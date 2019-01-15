using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Threading;
using System.Threading;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Data;

namespace mods
{
    class Filter
    {
        public string name;
        public string operation;
        public string value;
        public SqlDbType datatype;


        public static List<string> textbox_filters = new List<string> { "Version", "Job Name", "Version Int" };
        public static List<string> combobox_filters = new List<string> { "Fire Code" };

        public static Dictionary<string, string> filtersList = new Dictionary<string, string>
        {
            { "", "" },
            { "Version", "version" },
            { "Job Name", "jobname" },
            { "Fire Code", "firecode" },
            { "Version Int", "fileVersionInt" },
        };

        public static Dictionary<string, List<string>> filter_allowed_operations = new Dictionary<string, List<string>>
        {
            {"Version", new List<string>{"==","!=","Contains","Does Not Contain"}},
            {"Job Name", new List<string>{"==","!=","Contains","Does Not Contain"}},
            {"Fire Code", new List<string>{"==","!=","Contains","Does Not Contain"}},
            {"Version Int", new List<string>{"==","!="}},
        };

        public static Dictionary<string, SqlDbType> filter_data_types = new Dictionary<string, SqlDbType>
        {
            { "Version", SqlDbType.Text },
            { "Job Name", SqlDbType.Text },
            { "Fire Code", SqlDbType.Text },
            { "Version Int", SqlDbType.Int },
        };

        public static Dictionary<string, List<string>> filter_combobox_options = new Dictionary<string, List<string>>
        {
            {"Fire Code", new List<string>{"Chicago", "Chicago 2001", "Australia", "Detroit", "Hawaii", "Massachusets 2K", "New York City", "ANSI 2K", "ANSI 85-89 or 96"}},
        };

        public Filter(StackPanel sp)
        {
            name = ((ComboBox)sp.Children[0]).SelectedItem.ToString();
            operation = ((ComboBox)sp.Children[1]).SelectedItem.ToString();
            if(sp.Children[2].GetType() == typeof(ComboBox))
            {
                value = ((ComboBox)sp.Children[2]).SelectedItem.ToString();
            }
            else if (sp.Children[2].GetType() == typeof(TextBox))
            {
                value = ((TextBox)sp.Children[2]).Text;
            }
            datatype = filter_data_types[name];
        }

        public static string Filter_To_SQL(Filter filter)
        {
            string param = filtersList[filter.name];
            
            if (filter.operation == "==")
            {
                if (filter.datatype == SqlDbType.Int)
                    return param + " = @" + param;
                else
                    return param + " LIKE @" + param;
            }
            else if(filter.operation == "!=")
            {
                if(filter.datatype == SqlDbType.Int)
                    return param + " <> @" + param;
                else
                    return param + " NOT LIKE @" + param;
            }
            else if(filter.operation == "Contains")
            {
                filter.value = "%" + filter.value + "%";
                return param + " LIKE @" + param;
            }
            else if (filter.operation == "Does Not Contain")
            {
                filter.value = "%" + filter.value + "%";
                return param + " NOT LIKE @" + param;
            }
            else
            {
                return "";
            }
        }

        public static Control Get_Value_Type()
        {
            return null;
        }

        public static void Add_Filter_To_SP(ComboBox cb, StackPanel sp)
        {
            string filter = cb.SelectedItem.ToString();
            int remove_count = sp.Children.Count;

            for (int i = 1; i < remove_count; i++)
            {
                sp.Children.RemoveAt(1);   //Leave at 1 because the child count is reduced after every iteration
            }
            
            if (filter == "")
            {
                sp.Children.Clear();
                return;
            }

            ComboBox operation = new ComboBox
            {
                Width = 100,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Height = 25,
                Margin = new Thickness(0, 0, 0, 0),
            };

            foreach(string op in filter_allowed_operations[filter])
            {
                operation.Items.Add(op);
            }

            operation.SelectedIndex = 0;
            sp.Children.Add(operation);

            if (textbox_filters.Contains(filter))
            {
                TextBox value = new TextBox
                {
                    Width = 150,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Height = 25,
                    Margin = new Thickness(0, 0, 0, 0),
                };

                sp.Children.Add(value);
            }

            else if (combobox_filters.Contains(filter))
            {
                ComboBox value = new ComboBox
                {
                    Width = 150,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    Height = 25,
                    Margin = new Thickness(0, 0, 0, 0),
                };

                foreach(string option in filter_combobox_options[filter])
                {
                    value.Items.Add(option);
                }

                sp.Children.Add(value);
            }
        }

    }
}
