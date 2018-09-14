﻿using System;
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
using System.Security.Cryptography;
using WpfChosenControl.model;

namespace Healthtechbd
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Custom Session.............................
        public class Session
        {
            #region Doctor Info
            public static int doctorId;
            public static string userFirstName;                      
            public static string userLastName;                      
            public static int userPhone;                      
            public static string userEmail;
            #endregion

            public static int editRecordId;                    
        }

        public MainWindow()
        {
            InitializeComponent();            
        }

        public MainWindow(ResetPasswordWindow resetPasswordWindow) : this()
        {
            this.resetPasswordWindow = resetPasswordWindow;
        }

        contextd_db db = new contextd_db();
        user user = new user();
        private ResetPasswordWindow resetPasswordWindow;

        private void registrationLink_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Hide();
            RegistrationWindow registrationWindow = new RegistrationWindow(this);
            registrationWindow.Show();
        }

        private void forgotPasswordLink_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Hide();
            ForgotPasswordWindow forgotPasswordWindow = new ForgotPasswordWindow(this);
            forgotPasswordWindow.Show();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }
       
        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            //this.Hide();
            //AdminPanelWindow adminpanelwindow = new AdminPanelWindow(this);
            //adminpanelwindow.Show();

            if (EmailAddress.Text != "" && Password.Password != "")
            {
                try
                {
                    user = db.users.FirstOrDefault(x => x.email == EmailAddress.Text && x.password == Password.Password);

                    if (user != null) //User = Doctor
                    {
                        this.Hide();
                        AdminPanelWindow adminpanelwindow = new AdminPanelWindow(this);
                        adminpanelwindow.Show();
                        
                        //Doctor Info Save to Session....
                        Session.doctorId = user.id;
                        Session.userFirstName = user.first_name;
                        Session.userLastName = user.last_name;
                        Session.userEmail = user.email;
                        
                        if (MessageBox.Show("Login successfully", "Success") == MessageBoxResult.OK)
                        {
                            TextBlock UserName = AdminPanelWindow.userName;
                            UserName.Text = Session.userFirstName +" "+ Session.userLastName;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Email or Password are invalid", "Invalid User", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch
                {
                    MessageBox.Show("There is a problem, Please try again", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Please fill the all field", "Required field", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }        


        //Placeholder.................................................
        private void EmailAddress_GotFocus(object sender, RoutedEventArgs e)
        {
            if(EmailAddress.Text == "Email Address")
            {
                EmailAddress.Text = "";
            }                   
        }

        private void EmailAddress_LostFocus(object sender, RoutedEventArgs e)
        {
            if (EmailAddress.Text == "")
            {
                EmailAddress.Text = "Email Address";
            }           
        }     

        private void Password_GotFocus(object sender, RoutedEventArgs e)
        {
            if(Password.Password == "Password")
            {             
                Password.Password = "";
            }                       
        }

        private void Password_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Password.Password == "")
            {
                Password.Password = "Password";
            }
        }       
    }
}
