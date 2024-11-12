using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.Xml.Linq;

namespace TorusGateway.Torus
{
    /// <summary>
    /// MachineListControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MachineListControl : UserControl
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
#pragma warning disable SYSLIB1054 // 컴파일 타임에 P/Invoke 마샬링 코드를 생성하려면 'DllImportAttribute' 대신 'LibraryImportAttribute'를 사용하세요.
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
#pragma warning restore SYSLIB1054 // 컴파일 타임에 P/Invoke 마샬링 코드를 생성하려면 'DllImportAttribute' 대신 'LibraryImportAttribute'를 사용하세요.

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        //설정 불러오기용 함수
        public static string ReadConfig(string _settingFileName, string _section, string _key, string _defaultValue = "")
        {
            StringBuilder temp = new(255);
            _ = GetPrivateProfileString(_section, _key, null, temp, 255, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _settingFileName));
            if (temp.ToString() == "")
            {
                WriteConfig(_settingFileName, _section, _key, _defaultValue);
                return _defaultValue;
            }
            return temp.ToString();
        }
        //설정 저장용 함수
        public static void WriteConfig(string _settingFileName, string _section, string _key, string _contents)
        {
            WritePrivateProfileString(_section, _key, _contents, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _settingFileName));
        }

        private ObservableCollection<NCmachine> ncMachineList_ { get; set; } = [];

        public MachineListControl()
        {
            InitializeComponent();
            ListViewMachineList.ItemsSource = ncMachineList_;
            TextBoxTorusRunPath.Text = ReadConfig(Constants.ConfigFileName, "Main", "TorusRunPath", "");
            TextBoxTorusExitPath.Text = ReadConfig(Constants.ConfigFileName, "Main", "TorusExitPath", "");
            TextBoxTorusMachineListPath.Text = ReadConfig(Constants.ConfigFileName, "Main", "TorusMachineListPath", "");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (sender == ButtonTorusRunPath)
                {
                    if (FileSelector(TextBoxTorusRunPath.Text, out string newPath))
                    {
                        WriteConfig(Constants.ConfigFileName, "Main", "TorusRunPath", newPath);
                        TextBoxTorusRunPath.Text = newPath;
                    }
                }
                else if (sender == ButtonTorusExitPath)
                {
                    if (FileSelector(TextBoxTorusExitPath.Text, out string newPath))
                    {
                        WriteConfig(Constants.ConfigFileName, "Main", "TorusExitPath", newPath);
                        TextBoxTorusExitPath.Text = newPath;
                    }
                }
                else if (sender == ButtonTorusMachineListPath)
                {
                    if (FileSelector(TextBoxTorusMachineListPath.Text, out string newPath))
                    {
                        WriteConfig(Constants.ConfigFileName, "Main", "TorusMachineListPath", newPath);
                        TextBoxTorusMachineListPath.Text = newPath;
                    }
                }
                else if (sender == ButtonTorusRunExecute)
                {
                    RunProcess(TextBoxTorusRunPath.Text);
                }
                else if (sender == ButtonTorusExitExecute)
                {
                    RunProcess(TextBoxTorusExitPath.Text);
                }
                else if (sender == ButtonTorusMachineListRead)
                {
                    MachineListXmlRead(TextBoxTorusMachineListPath.Text);
                }
                else if (sender == ButtonTorusMachineListWrite)
                {
                    MachineListXmlWrite(TextBoxTorusMachineListPath.Text);
                }
                else if (sender == ButtonTorusMachineListItemSave)
                {
                    SaveMachineListItem();
                }
                else if (sender == ButtonTorusMachineListItemCancel)
                {
                    CancelMachineListItem();
                }
                else if (sender == ButtonTorusMachineListItemAdd)
                {
                    AddMachineListItem();
                }
                else if (sender == ButtonTorusMachineListItemRemove)
                {
                    RemoveMachineListItem();
                }
                else if (sender == ButtonTorusMachineListArrange)
                {
                    ArrangeMachineListItem();
                }
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == ListViewMachineList)
            {
                if (ListViewMachineList.SelectedItem is not NCmachine ncMachine)
                {
                    ClearMachineListItem();
                }
                else
                {
                    LoadMachineListItem();
                }
            }
        }

        private bool FileSelector(string oldPath, out string newPath)
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = "파일 열기",
                Filter = "모든 파일 (*.*)|*.*",
            };
            if (oldPath != "")
            {
                string absolutePath = System.IO.Path.GetFullPath(oldPath);
                string dirPath = System.IO.Path.GetDirectoryName(absolutePath);
                if (dirPath != null)
                {
                    openFileDialog.InitialDirectory = dirPath;
                }
            }
            if (openFileDialog.ShowDialog() == true)
            {
                newPath = openFileDialog.FileName;
                return true;
            }
            newPath = "";
            return false;
        }

        public static void RunProcess(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                // ProcessStartInfo 설정
                ProcessStartInfo processStartInfo = new()
                {
                    FileName = filePath, // 실행할 배치 파일
                    WorkingDirectory = System.IO.Path.GetDirectoryName(filePath), // 배치 파일이 있는 폴더
                    UseShellExecute = true // Windows 셸을 사용하여 실행
                };
                // Process 실행
                using Process process = new();
                process.StartInfo = processStartInfo;
                process.Start();
                process.WaitForExit(); // 프로세스가 종료될 때까지 기다림
            }
            else
            {
                MessageBox.Show("실행 파일을 찾을 수 없습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MachineListXmlRead(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                MessageBox.Show("MachineList.xml 파일을 찾을 수 없습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!filePath.EndsWith("MachineList.xml"))
            {
                MessageBox.Show("파일 이름이 \"MachineList.xml\"이어야 합니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ncMachineList_.Clear();
            XDocument doc = XDocument.Load(filePath);
            if (doc.Root.Name.ToString().Equals("machinelist", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var element in doc.Root.Elements())
                {
                    if (element.Name.ToString().Equals("ncmachine", StringComparison.InvariantCultureIgnoreCase))
                    {
                        bool isPass = false;
                        NCmachine ncMachine = new();
                        foreach (var attribute in element.Attributes())
                        {
                            string attributeName = attribute.Name.ToString().ToLowerInvariant();
                            string attributeValue = attribute.Value.ToString();
                            if (attributeName == "activate")
                            {
                                if (attributeValue.ToLowerInvariant() == "true")
                                {
                                    ncMachine.activate = true;
                                }
                                else
                                {
                                    ncMachine.activate = false;
                                }
                            }
                            else if (attributeName == "name")
                            {
                                ncMachine.name = attributeValue;
                            }
                            else if (attributeName == "id")
                            {
                                if (int.TryParse(attributeValue, out int id))
                                {
                                    ncMachine.id = id;
                                }
                                else
                                {
                                    isPass = true;
                                    break;
                                }
                            }
                            else if (attributeName == "vendorcode")
                            {
                                ncMachine.vendorCode = attributeValue;
                            }
                            else if (attributeName == "address")
                            {
                                ncMachine.address = attributeValue;
                            }
                            else if (attributeName == "port")
                            {
                                if (int.TryParse(attributeValue, out int port))
                                {
                                    ncMachine.port = port;
                                }
                                else
                                {
                                    isPass = true;
                                    break;
                                }
                            }
                            else if (attributeName == "exdllpath")
                            {
                                ncMachine.exDllPath = attributeValue;
                            }
                            else if (attributeName == "connectcode")
                            {
                                ncMachine.connectCode = attributeValue;
                            }
                            else if (attributeName == "ncversioncode")
                            {
                                ncMachine.ncVersionCode = attributeValue;
                            }
                            else if (attributeName == "toolsystem")
                            {
                                ncMachine.toolSystem = attributeValue;
                            }
                            else if (attributeName == "username")
                            {
                                ncMachine.username = attributeValue;
                            }
                            else if (attributeName == "password")
                            {
                                ncMachine.password = attributeValue;
                            }
                        }
                        if (isPass)
                        {
                            continue;
                        }
                        ncMachineList_.Add(ncMachine);
                    }
                }
            }
            ClearMachineListItem();
            ListViewMachineList.Items.Refresh();
        }

        private void MachineListXmlWrite(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                MessageBox.Show("MachineList.xml 파일을 찾을 수 없습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!filePath.EndsWith("MachineList.xml"))
            {
                MessageBox.Show("파일 이름이 \"MachineList.xml\"이어야 합니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var activeIds = ncMachineList_
                .Where(m => m.activate)
                .Select(m => m.id)
                .ToList();

            var duplicateId = activeIds
                .GroupBy(id => id)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .FirstOrDefault();

            if (duplicateId != 0)
            {
                MessageBox.Show($"Activate가 true인 설비 목록 중에 중복된 id({duplicateId})가 발견되었습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            XElement rootElement = new XElement("MachineList");
            foreach (var ncMachine in ncMachineList_)
            {
                XElement machineElement = new XElement("NCmachine",
                    new XAttribute("activate", ncMachine.activate.ToString().ToLowerInvariant()),
                    new XAttribute("name", ncMachine.name),
                    new XAttribute("id", ncMachine.id),
                    new XAttribute("vendorCode", ncMachine.vendorCode),
                    new XAttribute("address", ncMachine.address),
                    new XAttribute("port", ncMachine.port),
                    new XAttribute("exDllPath", ncMachine.exDllPath),
                    new XAttribute("connectCode", ncMachine.connectCode),
                    new XAttribute("ncVersionCode", ncMachine.ncVersionCode),
                    new XAttribute("toolSystem", ncMachine.toolSystem),
                    new XAttribute("username", ncMachine.username),
                    new XAttribute("password", ncMachine.password)
                );
                rootElement.Add(machineElement);
            }
            XDocument doc = new(rootElement);
            doc.Save(filePath);
            MessageBox.Show("MachineList.xml 파일을 변경하였습니다.", "성공", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveMachineListItem()
        {
            if (ListViewMachineList.SelectedItem is not NCmachine ncMachine)
            {
                return;
            }
            if (TextBoxTorusNcMachineName.Text.Trim() == "")
            {
                MessageBox.Show("name은 비어있을 수 없습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!int.TryParse(TextBoxTorusNcMachineId.Text, out int id) || id < 1)
            {
                MessageBox.Show("id는 1 이상의 정수여야 합니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!int.TryParse(TextBoxTorusNcMachinePort.Text, out int port) || id < 0 || id > 65535)
            {
                MessageBox.Show("port는 0 이상, 65535 이하의 정수여야 합니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string activate = ((ComboBoxItem)ComboBoxTorusNcMachineActivate.SelectedItem)?.Content.ToString().ToLowerInvariant();
            if (activate == "true")
            {
                ncMachine.activate = true;
            }
            else
            {
                ncMachine.activate = false;
            }
            ncMachine.name = TextBoxTorusNcMachineName.Text.Trim();
            ncMachine.id = id;
            ncMachine.vendorCode = ((ComboBoxItem)ComboBoxTorusNcMachineVendorCode.SelectedItem)?.Content.ToString();
            ncMachine.address = TextBoxTorusNcMachineAddress.Text.Trim();
            ncMachine.port = port;
            ncMachine.exDllPath = TextBoxTorusNcMachineExDllPath.Text.Trim();
            ncMachine.connectCode = ((ComboBoxItem)ComboBoxTorusNcMachineConnectCode.SelectedItem)?.Content.ToString();
            ncMachine.ncVersionCode = ((ComboBoxItem)ComboBoxTorusNcMachineNcVersionCode.SelectedItem)?.Content.ToString();
            ncMachine.toolSystem = ((ComboBoxItem)ComboBoxTorusNcMachineToolSystem.SelectedItem)?.Content.ToString();
            ncMachine.username = TextBoxTorusNcMachineUsername.Text.Trim();
            ncMachine.password = TextBoxTorusNcMachinePassword.Text.Trim();
            ListViewMachineList.Items.Refresh();
        }

        private void CancelMachineListItem()
        {
            if (ListViewMachineList.SelectedItem is not NCmachine ncMachine)
            {
                ClearMachineListItem();
            }
            else
            {
                LoadMachineListItem();
            }
        }

        private void AddMachineListItem()
        {
            NCmachine machine = new();
            machine.name = "Machine";
            machine.id = 1;
            machine.vendorCode = "Kcnc";
            machine.connectCode = "Default";
            machine.ncVersionCode = "Default";
            machine.toolSystem = "Default";
            ncMachineList_.Add(machine);
            ListViewMachineList.Items.Refresh();
            ListViewMachineList.SelectedItem = machine;
            ListViewMachineList.ScrollIntoView(machine);
        }

        private void RemoveMachineListItem()
        {
            if (ListViewMachineList.SelectedItem is not NCmachine ncMachine)
            {
                return;
            }
            ncMachineList_.Remove(ncMachine);
            ListViewMachineList.Items.Refresh();
            ClearMachineListItem();
        }

        private void ArrangeMachineListItem()
        {
            var selectedItem = ListViewMachineList.SelectedItem as NCmachine;
            var sortedList = new ObservableCollection<NCmachine>(ncMachineList_.OrderBy(x => x.id));
            ncMachineList_ = sortedList;
            ListViewMachineList.ItemsSource = ncMachineList_;
            ListViewMachineList.Items.Refresh();
        }

        private void ClearMachineListItem()
        {
            ComboBoxTorusNcMachineActivate.SelectedIndex = 1;
            TextBoxTorusNcMachineName.Text = "";
            TextBoxTorusNcMachineId.Text = "";
            ComboBoxTorusNcMachineVendorCode.SelectedIndex = 4;
            TextBoxTorusNcMachineAddress.Text = "";
            TextBoxTorusNcMachinePort.Text = "";
            TextBoxTorusNcMachineExDllPath.Text = "";
            ComboBoxTorusNcMachineConnectCode.SelectedIndex = 0;
            ComboBoxTorusNcMachineNcVersionCode.SelectedIndex = 0;
            ComboBoxTorusNcMachineToolSystem.SelectedIndex = 0;
            TextBoxTorusNcMachineUsername.Text = "";
            TextBoxTorusNcMachinePassword.Text = "";
        }

        private void LoadMachineListItem()
        {
            if (ListViewMachineList.SelectedItem is not NCmachine ncMachine)
            {
                return;
            }
            string targetValue = ncMachine.activate.ToString();
            foreach (ComboBoxItem item in ComboBoxTorusNcMachineActivate.Items)
            {
                if (string.Equals(item.Content.ToString(), targetValue, StringComparison.OrdinalIgnoreCase))
                {
                    ComboBoxTorusNcMachineActivate.SelectedItem = item;
                    break;
                }
            }
            targetValue = ncMachine.name;
            TextBoxTorusNcMachineName.Text = targetValue;
            targetValue = ncMachine.id.ToString();
            TextBoxTorusNcMachineId.Text = targetValue;
            targetValue = ncMachine.vendorCode;
            foreach (ComboBoxItem item in ComboBoxTorusNcMachineVendorCode.Items)
            {
                if (string.Equals(item.Content.ToString(), targetValue, StringComparison.OrdinalIgnoreCase))
                {
                    ComboBoxTorusNcMachineVendorCode.SelectedItem = item;
                    break;
                }
            }
            targetValue = ncMachine.address;
            TextBoxTorusNcMachineAddress.Text = targetValue;
            targetValue = ncMachine.port.ToString();
            TextBoxTorusNcMachinePort.Text = targetValue;
            targetValue = ncMachine.exDllPath;
            TextBoxTorusNcMachineExDllPath.Text = targetValue;
            targetValue = ncMachine.connectCode;
            if (targetValue.Trim() == "")
            {
                targetValue = "Default";
            }
            foreach (ComboBoxItem item in ComboBoxTorusNcMachineConnectCode.Items)
            {
                if (string.Equals(item.Content.ToString(), targetValue, StringComparison.OrdinalIgnoreCase))
                {
                    ComboBoxTorusNcMachineConnectCode.SelectedItem = item;
                    break;
                }
            }
            targetValue = ncMachine.ncVersionCode;
            if (targetValue.Trim() == "")
            {
                targetValue = "Default";
            }
            foreach (ComboBoxItem item in ComboBoxTorusNcMachineNcVersionCode.Items)
            {
                if (string.Equals(item.Content.ToString(), targetValue, StringComparison.OrdinalIgnoreCase))
                {
                    ComboBoxTorusNcMachineNcVersionCode.SelectedItem = item;
                    break;
                }
            }
            targetValue = ncMachine.toolSystem;
            if (targetValue.Trim() == "")
            {
                targetValue = "Default";
            }
            foreach (ComboBoxItem item in ComboBoxTorusNcMachineToolSystem.Items)
            {
                if (string.Equals(item.Content.ToString(), targetValue, StringComparison.OrdinalIgnoreCase))
                {
                    ComboBoxTorusNcMachineToolSystem.SelectedItem = item;
                    break;
                }
            }
            targetValue = ncMachine.username;
            TextBoxTorusNcMachineUsername.Text = targetValue;
            targetValue = ncMachine.password;
            TextBoxTorusNcMachinePassword.Text = targetValue;
        }
    }
}
