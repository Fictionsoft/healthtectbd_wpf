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

namespace Healthtechbd
{
    /// <summary>
    /// Interaction logic for AddMedicines.xaml
    /// </summary>
    public partial class AddMedicines : Page
    {
        public AddMedicines()
        {
            InitializeComponent();
        }

        model.ContextDb db = new model.ContextDb();
        model.medicine medicine = new model.medicine();

        private void SubmitAddMedicine_Click(object sender, RoutedEventArgs e)
        {
            if(medicineName.Text != "")
            {
                NavigationService.Navigate(new Uri("Medicines.xaml", UriKind.Relative));

                medicine.name = medicineName.Text.Trim();
                db.medicines.Add(medicine);
                medicineName.Clear();
                db.SaveChanges();

                MessageBox.Show("Medicine Save Successfully");
            }
            else
            {
                MessageBox.Show("Medicine name is required", "Required field", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelAddMedicine_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("Medicines.xaml", UriKind.Relative));
        }
    }
}
