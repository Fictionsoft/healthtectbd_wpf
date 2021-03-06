﻿using Healthtechbd.Model;
using Healthtechbd.Model.ApiModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfChosenControl.model;

namespace Healthtechbd
{
    /// <summary>
    /// Interaction logic for Patients.xaml
    /// </summary>
    public partial class Patients : Page
    {
        public Patients()
        {
            InitializeComponent();
            loadPatients();
        }

        //From Dashboard 
        public Patients(List<user> patients) : this()
        {
            dataGridPatients.ItemsSource = patients; //Patients = Users   
        }

        contextd_db db = new contextd_db();
        user user = new user();
        prescription prescription = new prescription();

        void loadPatients() //User = Patient
        {
            try
            {               
                var patients = db.users.Where(x => x.role_id == 3 && x.doctor_id == MainWindow.Session.doctor_id).OrderByDescending(x => x.created).Take(40).ToList();// role_id 3 = Patient
                dataGridPatients.ItemsSource = patients; //Patients = Users                
            }
            catch
            {
                MessageBox.Show("There is a problem, Please try again", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ButtonAddPatient_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("AddPatient.xaml", UriKind.Relative));
        }

        private void btnDeletePatientRow_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are You Sure ?", "Confirm",
               MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    int patientId = (dataGridPatients.SelectedItem as user).id;
                    user = db.users.FirstOrDefault(x => x.id == patientId);

                    db.users.Remove(user);
                    db.SaveChanges();
                    loadPatients();
                    MessageBox.Show("Delete Successfully", "Success");
                }
                catch
                {
                    MessageBox.Show("There is a problem, Please try again", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void btnEditPatientRow_Click(object sender, RoutedEventArgs e)
        {
            int patientId = (dataGridPatients.SelectedItem as user).id;
            EditPatient editPatient = new EditPatient(patientId);
            NavigationService.Navigate(editPatient);
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            searchField.Clear();
            loadPatients();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            search();
        }

        public void search()
        {
            if (searchField.Text != "")
            {
                string searchBy = searchField.Text.ToString();

                try
                {
                    var users = db.users.Where(x => (x.role_id == 3 && x.doctor_id == MainWindow.Session.doctor_id) &&
                                                (x.first_name.Contains(searchBy) ||
                                                x.last_name.Contains(searchBy) ||
                                                x.phone.Contains(searchBy) ||
                                                x.email.Contains(searchBy) ||
                                                x.age.Contains(searchBy))
                                               ).Take(40).ToList();

                    dataGridPatients.ItemsSource = users;

                }
                catch
                {
                    MessageBox.Show("There is a problem, Please try again", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void btnViewPatientPrescription_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("Prescriptions.xaml", UriKind.Relative));

            MainWindow.Session.set_patient_id = (dataGridPatients.SelectedItem as user).id;
        }

        private void btnCreatePatientPrescription_Click(object sender, RoutedEventArgs e)
        {
            Grid sidebar = AdminPanelWindow.sidebar;
            sidebar.Visibility = Visibility.Hidden;
            AdminPanelWindow.sidebarColumnDefination.Width = new GridLength(0);

            int patientId = (dataGridPatients.SelectedItem as user).id;

            AddPrescription addPrescription = new AddPrescription(patientId);            
            NavigationService.Navigate(addPrescription);
        }

        public static int offline_total;
        public static int offline_success;
        public static int offline_duplicate;

        public static int online_total;
        public static int online_success;
        public static int online_duplicate;

        private void ButtonSyncPatient_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.Internet.CheckForInternetConnection() == true)
            {
                GetonlinePatients();
                GetLocalPatients();
                loadPatients();
                MessageBox.Show("Online to offline: \n Total : " + offline_total + "\n Success : " + offline_success + "\n Duplicate : " + offline_duplicate
                                 + "\n \n Offline to online: \n Total : " + online_total + "\n Success : " + online_success + "\n Duplicate : " + online_duplicate,
                                 "Patients sync report", MessageBoxButton.OK);
                

                offline_total = offline_success = offline_duplicate = online_total = online_success = online_duplicate = 0; // Clear Value
            }
            else
            {
                MessageBox.Show("Check your internet connection, Please try again", "Warning");
            }
        }

        public async void GetonlinePatients()
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(MainWindow.Session.api_base_url);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.GetAsync("admin/users/get-online-patients?doctor_email=" + MainWindow.Session.doctor_email).Result;
                if (response.IsSuccessStatusCode)
                {
                    var patients = response.Content.ReadAsStringAsync();
                    patients.Wait();
                    var online_patients = JsonConvert.DeserializeObject<List<ViewPatients>>(patients.Result);

                    //Count Total Sync Patients From on-line
                    offline_total = online_patients.Count();

                    if (offline_total > 0)
                    {
                        if (SaveonlinePatientsToLocal(online_patients))
                        {
                            HttpResponseMessage change_is_sync_response = client.PostAsJsonAsync("admin/users/get-online-patients", online_patients).Result;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Error Code " + response.StatusCode + " : Message - " + response.ReasonPhrase);
                }            
            }
            catch
            {
                MessageBox.Show("There is a problem, Please try again", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }            
        }

        public bool SaveonlinePatientsToLocal(List<ViewPatients> online_patients)
        {            
            foreach (var patient in online_patients)
            {
                var have_patient = db.users.Where(x => x.first_name == patient.first_name && x.phone == patient.phone && x.doctor_id == MainWindow.Session.doctor_id).FirstOrDefault();

                if(have_patient != null)
                {
                    have_patient.is_sync = 1;
                    db.SaveChanges();

                    //Count Duplicate Sync Patients From on-line
                    offline_duplicate++;
                }
                else
                {                   
                    user.doctor_id = MainWindow.Session.doctor_id;
                    user.role_id = patient.role_id; // role_id 3 = Patient
                    user.first_name = patient.first_name;
                    user.phone = patient.phone;
                    user.email = patient.email;
                    user.age = patient.age;
                    user.address_line1 = patient.address_line1;
                    user.expire_date = patient.expire_date;
                    user.is_sync = 1;
                    user.created = DateTime.Now;

                    db.users.Add(user);
                    db.SaveChanges();

                    //Count Success Sync Patients From on-line
                    offline_success++;
                }                                              
            }

            if(offline_total > 0 || offline_duplicate > 0)
            {
                return true;
            }
            return false;
        }

        public async void GetLocalPatients()
        {
            try
            {
                var local_patients = db.users
                                    .Where(x => x.role_id == 3 && x.doctor_id == MainWindow.Session.doctor_id && x.is_sync == 0)
                                    .Take(100)
                                    .ToList();// role_id 3 = Patient                              
            
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(MainWindow.Session.api_base_url);

                if(local_patients.Count() > 0)
                {
                    HttpResponseMessage response = client.PostAsJsonAsync("admin/users/get-local-patients?doctor_email=" + MainWindow.Session.doctor_email, local_patients).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var response_local_patient_save_to_online = response.Content.ReadAsStringAsync();
                        response_local_patient_save_to_online.Wait();

                        var online_response = JsonConvert.DeserializeObject<SuccessMessages>(response_local_patient_save_to_online.Result);

                        if (online_response.status == "success")
                        {
                            online_total = online_response.online_total;
                            online_success = online_response.online_success;
                            online_duplicate = online_response.online_duplicate;

                            ChangeIsSyncLocalPatients(local_patients);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Error Code " + response.StatusCode + " : Message - " + response.ReasonPhrase);
                    }
                }
            }
            catch
            {
                MessageBox.Show("There is a problem, Please try again", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void ChangeIsSyncLocalPatients(List<user> LocalPatients)
        {
            foreach (var LocalPatient in LocalPatients)
            {
                LocalPatient.is_sync = 1;
                db.SaveChanges();
            }
        }
    }
}
