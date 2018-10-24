﻿using Healthtechbd.Model;
using System;
using System.Collections.Generic;
using System.Linq;
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
using WpfChosenControl;
using WpfChosenControl.model;

namespace Healthtechbd
{
    /// <summary>
    /// Interaction logic for EditPrescription.xaml
    /// </summary>
    public partial class EditPrescription : Page
    {
        private DiagnosisMedicineChosenControl diagnosisMedicineChosenControl = null;
        private DiagnosisTestChosenControl diagnosisTestChosenControl = null;

        public EditPrescription()
        {
            InitializeComponent();

            diagnosisMedicineChosenControl = (DiagnosisMedicineChosenControl)FindName("medicineChosen");
            diagnosisTestChosenControl = (DiagnosisTestChosenControl)FindName("testChosen");

            LoadPatientCombobox();
            LoadDiagnosisCheckbox();

            PatientComboBox.AddHandler(System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent,
                   new System.Windows.Controls.TextChangedEventHandler(PatientComboBox_TextChanged));
        }

        contextd_db db = new contextd_db();
        prescription prescription = new prescription();
        prescriptions_diagnosis prescriptions_diagnosi = new prescriptions_diagnosis();
        prescriptions_medicine prescriptions_medicine = new prescriptions_medicine();
        prescriptions_test prescriptions_test = new prescriptions_test();
        user patient = new user();

        public EditPrescription(int id) : this()
        {
            PrescriptionId.Text = id.ToString();

            try
            {               
                var prescription = db.presceiptions.FirstOrDefault(x => x.id == id);
                PatientComboBox.SelectedItem = prescription.user.first_name;
                PatientPhone.Text = prescription.user.phone;
                PatientAddress.Text = prescription.user.address_line1;
                PatientAge.Text = prescription.user.age;

                BloodPresure.Text = prescription.blood_pressure;
                Temperature.Text = prescription.temperature;

                PatientLastVisit.Text = db.presceiptions.Where(x => x.user_id == prescription.user.id).OrderByDescending(x => x.created).Select(x => x.created).FirstOrDefault().ToString("dd MMM yyyy");

                DoctorsNotes.Text = prescription.doctores_notes;
                OtherInstructions.Text = prescription.other_instructions;

                AllPrescription.Children.Clear();
                if (prescription.user.prescription.Count() > 0)
                {
                    foreach (var item in prescription.user.prescription)
                    {
                        TextBlock textBlock = new TextBlock();
                        textBlock.VerticalAlignment = VerticalAlignment.Center;
                        textBlock.Padding = new Thickness(10, 5, 10, 5);
                        textBlock.DataContext = item.id;
                        textBlock.Text = item.created.ToString("dd MMM yyyy");

                        textBlock.AddHandler(TextBlock.MouseDownEvent, new RoutedEventHandler(AllPrescriptionClick));

                        AllPrescription.Children.Add(textBlock);
                    }
                }
            }
            catch
            {
                MessageBox.Show("There is a problem, Please try again", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void PatientComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ComboBox obj = sender as ComboBox;

            var searchBy = obj.Text;

            var patients = db.users.Where(x => (x.role_id == 3 && x.doctor_id == MainWindow.Session.doctorId) &&
                           (x.first_name.Contains(searchBy))).OrderByDescending(x => x.created).Take(10).ToList(); //patient_id 3

            PatientComboBox.Items.Clear();

            foreach (var patient in patients)
            {
                PatientComboBox.Items.Add(patient.first_name);
            }

            if (patients.Count == 0)
            {
                PatientComboBox.Items.Add("No results mached with " + searchBy);
            }
        }

        private void PatientComboBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PatientComboBox.IsDropDownOpen = true;
        }

        //Patient Combobox DropDown Closed..........
        private void PatientComboBox_DropDownClosed(object sender, EventArgs e)
        {
            try
            {
                patient = db.users.FirstOrDefault(x => x.first_name == PatientComboBox.Text);

                if (patient != null)
                {
                    PatientPhone.Text = patient.phone;
                    PatientAddress.Text = patient.address_line1;
                    PatientAge.Text = patient.age;

                    PatientLastVisit.Text = db.presceiptions.Where(x => x.user_id == patient.id).OrderByDescending(x => x.created).Select(x => x.created).FirstOrDefault().ToString("dd MMM yyyy");
                }

                AllPrescription.Children.Clear();
                if (patient.prescription.Count() > 0)
                {
                    foreach (var prescription in patient.prescription)
                    {
                        TextBlock textBlock = new TextBlock();
                        textBlock.VerticalAlignment = VerticalAlignment.Center;
                        textBlock.Padding = new Thickness(10, 5, 10, 5);
                        textBlock.DataContext = prescription.id;
                        textBlock.Text = prescription.created.ToString("dd MMM yyyy");

                        textBlock.AddHandler(TextBlock.MouseDownEvent, new RoutedEventHandler(AllPrescriptionClick));

                        AllPrescription.Children.Add(textBlock);
                    }
                }
            }
            catch
            {
                MessageBox.Show("There is a problem, Please try again", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AllPrescriptionClick(object sender, RoutedEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;

            MainWindow.Session.editRecordId = int.Parse(textBlock.DataContext.ToString());

            Grid sidebar = AdminPanelWindow.sidebar;
            sidebar.Visibility = Visibility.Visible;

            AdminPanelWindow.sidebarColumnDefination.Width = new GridLength(242); // To set width 242 cause when I press AddPresscription it's Width set 0 (to remove sidebar/navigationbar).                           
            int doctorPrescriptionTemId = MainWindow.Session.doctorPrescriptionTemId;

            if (doctorPrescriptionTemId == 1)
            {
                PrescriptionTem = "StandardTemplate.xaml";
            }
            else if (doctorPrescriptionTemId == 2)
            {
                PrescriptionTem = "ClassicTemplate.xaml";
            }
            else if (doctorPrescriptionTemId == 3)
            {
                PrescriptionTem = "CustomTemplate.xaml";
            }
            else
            {
                PrescriptionTem = "GeneralTemplate.xaml";
            }

            NavigationService.Navigate(new Uri("prescriptionTemplates/" + PrescriptionTem, UriKind.Relative));
        }

        //Load Patient Combobox.....
        void LoadPatientCombobox()
        {
            try
            {
                var patients = db.users.Where(x => x.role_id == 3 && x.doctor_id == MainWindow.Session.doctorId).OrderByDescending(x => x.created).Take(10).ToList(); //patient_id 3

                foreach (var patient in patients)
                {
                    PatientComboBox.Items.Add(patient.first_name);
                }
            }
            catch
            {
                MessageBox.Show("There is a problem, Please try again", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //Load Diagnosis CheckBox.....
        void LoadDiagnosisCheckbox()
        {
            var diagnosis_templates = db.diagnosis_templates.ToList();
            foreach (var diagnosis_template in diagnosis_templates)
            {
                CheckBox checkbox = new CheckBox();
                checkbox.Content = diagnosis_template.diagnosis.name.ToString();
                checkbox.DataContext = diagnosis_template.id;
                DiagnosisCheckbox.Children.Add(checkbox);

                var exitingDiagnosisTemplate = db.prescriptions_diagnosis
                           .Where(x => x.diagnosis_id == diagnosis_template.id && x.prescription_id == MainWindow.Session.editRecordId)
                           .FirstOrDefault();

                if (exitingDiagnosisTemplate != null)
                {
                    checkbox.IsChecked = true;
                }
               
                checkbox.AddHandler(CheckBox.ClickEvent, new RoutedEventHandler(DiagnosIsClick));
            }
        }

        public static List<long> diagnosisTemplateIds = new List<long>();
        private void DiagnosIsClick(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;

            var diagnosisTemplateId = (int)checkBox.DataContext;

            if (checkBox.IsChecked == true)
            {
                diagnosisTemplateIds.Add(diagnosisTemplateId);
            }

            if (checkBox.IsChecked == false)
            {
                diagnosisTemplateIds.Remove(diagnosisTemplateId);
            }

            var diagnosis_templates = db.diagnosis_templates
                .Where(x => diagnosisTemplateIds.Contains(x.id))
                .Select(x => x.instructions).ToList();

            var instructions = "";
            foreach (var instruction in diagnosis_templates)
            {
                instructions += instruction + (instruction.Equals(diagnosis_templates.Last()) ? "." : ", ");
            }

            DoctorsNotes.Text = instructions;

            var diagnosisMedicines = db.diagnosis_medicines.Where(x => diagnosisTemplateIds.Contains(x.diagnosis_id))
                .Select(x => new IdNameModel
                {
                    Id = x.medicine_id,
                    Name = x.medicine.name
                })
                .ToList();

            var diagnosisTests = db.diagnosis_tests.Where(x => diagnosisTemplateIds.Contains(x.diagnosis_id))
                .Select(x => new IdNameModel
                {
                    Id = x.test_id,
                    Name = x.test.name
                })
                .ToList();

            ((DiagnosisMedicineModel)diagnosisMedicineChosenControl.DataContext).SelectedMedicines = diagnosisMedicines;
            ((DiagnosisTestModel)diagnosisTestChosenControl.DataContext).SelectedTests = diagnosisTests;

            // store mediciene ids to save into database
            DiagnosisMedicineChosenControl.selectedIds.Clear();
            foreach (var diagnosisMedicine in diagnosisMedicines)
            {
                DiagnosisMedicineChosenControl.selectedIds.Add(diagnosisMedicine.Id);
                diagnosisMedicineChosenControl._nodeList.Add(new Node(new IdNameModel() { Id = diagnosisMedicine.Id, Name = diagnosisMedicine.Name }));
            }

            // store test ids to save into database
            DiagnosisTestChosenControl.selectedIds.Clear();
            foreach (var diagnosisTest in diagnosisTests)
            {
                DiagnosisTestChosenControl.selectedIds.Add(diagnosisTest.Id);
                diagnosisTestChosenControl._nodeList.Add(new Node(new IdNameModel() { Id = diagnosisTest.Id, Name = diagnosisTest.Name }));
            }
        }

        string PrescriptionTem;
        private void UpdatePrescription_Click(object sender, RoutedEventArgs e)
        {
            if (PatientComboBox.Text != "" && PatientPhone.Text != "" && PatientAge.Text != "")
            {
                Grid sidebar = AdminPanelWindow.sidebar;
                sidebar.Visibility = Visibility.Visible;

                AdminPanelWindow.sidebarColumnDefination.Width = new GridLength(242); // To set width 242 cause when I press AddPresscription it's Width set 0 (to remove sidebar/navigationbar).            


                if (((Button)sender).Name == "UpdatePrescription")
                {
                    NavigationService.Navigate(new Uri("Prescriptions.xaml", UriKind.Relative));
                }
                else
                {
                    int doctorPrescriptionTemId = MainWindow.Session.doctorPrescriptionTemId;

                    if (doctorPrescriptionTemId == 1)
                    {
                        PrescriptionTem = "StandardTemplate.xaml";
                    }
                    else if (doctorPrescriptionTemId == 2)
                    {
                        PrescriptionTem = "ClassicTemplate.xaml";
                    }
                    else if (doctorPrescriptionTemId == 3)
                    {
                        PrescriptionTem = "CustomTemplate.xaml";
                    }
                    else
                    {
                        PrescriptionTem = "GeneralTemplate.xaml";
                    }

                    NavigationService.Navigate(new Uri("prescriptionTemplates/" + PrescriptionTem, UriKind.Relative));
                }

                try
                {
                    patient = db.users.FirstOrDefault(x => x.first_name == PatientComboBox.Text);

                    var havePhone = db.users.FirstOrDefault(x => x.phone == PatientPhone.Text && x.id != patient.id && x.doctor_id == MainWindow.Session.doctorId);

                    if (havePhone == null)
                    {                   
                        patient.first_name = PatientComboBox.Text.Trim();
                        patient.phone = PatientPhone.Text.Trim();
                        patient.age = PatientAge.Text.Trim();
                        patient.address_line1 = PatientAddress.Text.Trim();

                        db.SaveChanges();

                        prescription = db.presceiptions.FirstOrDefault(x => x.id == MainWindow.Session.editRecordId);

                        prescription.user_id = patient.id;
                        prescription.doctor_id = MainWindow.Session.doctorId; //doctorId = doctor_id
                        prescription.blood_pressure = BloodPresure.Text;
                        prescription.temperature = Temperature.Text;
                        prescription.doctores_notes = DoctorsNotes.Text;
                        prescription.other_instructions = OtherInstructions.Text;
                        prescription.status = true;
                        prescription.created = DateTime.Now;

                        int result_add_prescription = db.SaveChanges();

                        if (result_add_prescription > 0)
                        {
                            //Add Prescription Diagnosis
                            AddPrescriptionDiagnosis(MainWindow.Session.editRecordId);

                            //Add Prescription Medicines
                            AddPrescriptionMedicines(MainWindow.Session.editRecordId);

                            //Add Prescription Tests
                            AddPrescriptionTests(MainWindow.Session.editRecordId);

                            diagnosisTemplateIds.Clear();
                            DiagnosisMedicineChosenControl.selectedIds.Clear();
                            DiagnosisTestChosenControl.selectedIds.Clear();
                        }

                        MessageBox.Show("Prescription has been saved", "Success");
                    }
                    else
                    {
                        MessageBox.Show("The Phone already exist.", "Already Exit");
                    }                    
                }
                catch
                {
                    MessageBox.Show("There is a problem, Please try again.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Please fill in the required fields", "Required field", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        void AddPrescriptionDiagnosis(int PrescriptionId)
        {
            //prescription diagnosis delete
            var prescriptions_diagnosis = db.prescriptions_diagnosis.Where(x => x.prescription_id == PrescriptionId);
            if (prescriptions_diagnosis.Count() > 0)
            {
                db.prescriptions_diagnosis.RemoveRange(prescriptions_diagnosis);
                int delete_result = db.SaveChanges();
            }

            //prescription diagnosis add            
            foreach (int diagnosisTemplateId in diagnosisTemplateIds)
            {
                prescriptions_diagnosi.prescription_id = PrescriptionId;
                prescriptions_diagnosi.diagnosis_id = diagnosisTemplateId;
                prescriptions_diagnosi.status = true;
                prescriptions_diagnosi.created = DateTime.Now;
                db.prescriptions_diagnosis.Add(prescriptions_diagnosi);
                int retult_prescription_diagnosis = db.SaveChanges();
            }
        }

        void AddPrescriptionMedicines(int PrescriptionId)
        {
            //prescription medicines delete
            var prescriptions_medicines = db.prescriptions_medicines.Where(x => x.prescription_id == PrescriptionId);
            if (prescriptions_medicines.Count() > 0)
            {
                db.prescriptions_medicines.RemoveRange(prescriptions_medicines);
                int delete_result = db.SaveChanges();
            }

            //prescription medicines add
            var medicinesIds = DiagnosisMedicineChosenControl.selectedIds;
            foreach (int medicine_id in medicinesIds)
            {
                prescriptions_medicine.prescription_id = PrescriptionId;
                prescriptions_medicine.medicine_id = medicine_id;
                prescriptions_medicine.status = true;
                prescriptions_medicine.created = DateTime.Now;
                db.prescriptions_medicines.Add(prescriptions_medicine);
                int retult_prescription_medecines = db.SaveChanges();
            }
        }

        void AddPrescriptionTests(int PrescriptionId)
        {
            //prescription tests delete
            var prescriptions_tests = db.prescriptions_tests.Where(x => x.prescription_id == PrescriptionId);
            if (prescriptions_tests.Count() > 0)
            {
                db.prescriptions_tests.RemoveRange(prescriptions_tests);
                int delete_result = db.SaveChanges();
            }

            //prescription tests add
            var testsIds = DiagnosisTestChosenControl.selectedIds;
            foreach (int test_id in testsIds)
            {
                prescriptions_test.prescription_id = PrescriptionId;
                prescriptions_test.test_id = test_id;
                prescriptions_test.status = true;
                prescriptions_test.created = DateTime.Now;
                db.prescriptions_tests.Add(prescriptions_test);
                int retult_prescription_tests = db.SaveChanges();
            }
        }


        private void CancelUpdatePrescription_Click(object sender, RoutedEventArgs e)
        {
            Grid sidebar = AdminPanelWindow.sidebar;
            sidebar.Visibility = Visibility.Visible;

            AdminPanelWindow.sidebarColumnDefination.Width = new GridLength(242); // To set width 242 cause when I press AddPresscription it's Width set 0 (to remove sidebar/navigationbar).            

            NavigationService.Navigate(new Uri("Prescriptions.xaml", UriKind.Relative));
        }
    }
}