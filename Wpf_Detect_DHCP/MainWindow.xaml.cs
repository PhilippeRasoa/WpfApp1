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
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;

namespace NetDiag
{
    class Program
    {
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto)]
        private static extern int GetBestInterface(UInt32 destAddr, out UInt32 bestIfIndex);

        public static string GetGatewayForDestination(IPAddress destinationAddress)
        {
            UInt32 destaddr = BitConverter.ToUInt32(destinationAddress.GetAddressBytes(), 0);

            uint interfaceIndex;
            int result = GetBestInterface(destaddr, out interfaceIndex);
            if (result != 0)
                throw new Win32Exception(result);

            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                var niprops = ni.GetIPProperties();
                if (niprops == null)
                    continue;

                var gateway = niprops.GatewayAddresses?.FirstOrDefault()?.Address;
                if (gateway == null)
                    continue;

                if (ni.Supports(NetworkInterfaceComponent.IPv4))
                {
                    var v4props = niprops.GetIPv4Properties();
                    if (v4props == null)
                        continue;

                    if (v4props.Index == interfaceIndex)
                        return gateway.ToString();
                }

                if (ni.Supports(NetworkInterfaceComponent.IPv6))
                {
                    var v6props = niprops.GetIPv6Properties();
                    if (v6props == null)
                        continue;

                    if (v6props.Index == interfaceIndex)
                        return gateway.ToString();
                }
            }

            return null;
        }

        public static bool ShowIPAddressesBool()
        {
            string s = "";
            bool dhcpEnabled;
            DateTime localDate = DateTime.Now;
            //s += "Analyse du réseau de ce PC en cours...\n";
            //s += "============================================ \n";
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            // for all network interfaces
            foreach (NetworkInterface adapter in nics)
            {
                // get only Ethernet and active adapters (VM included)
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet && adapter.OperationalStatus == OperationalStatus.Up)
                {
                    //get its properties
                    IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                    IPv4InterfaceProperties p = adapterProperties.GetIPv4Properties();

                    // Network interface description
                    //s += nic0.Description + "\n";

                    // Check if its DHCP is enabled
                    if (p.IsDhcpEnabled)
                    {
                        //get its properties
                        UnicastIPAddressInformationCollection uniCast = adapterProperties.UnicastAddresses;
                        GatewayIPAddressInformationCollection addresses = adapterProperties.GatewayAddresses;
                        GatewayIPAddressInformation address0 = addresses[0];
                        //s += nic0.Description + " DHCP activé pour cette carte " + p.IsDhcpEnabled + "\n";
                        IPAddress ipDestination = new IPAddress(0x0); // route par défaut 

                        if (GetGatewayForDestination(ipDestination) == address0.Address.ToString()) //checks if NIC uses default route for Internet 
                        {
                            //s += "ipDestination = " + GetGatewayForDestination(ipDestination) + "\n";
                            //s += "passerelle par défaut : " + addresses[0].Address.ToString() + "\n\n";

                            string lifeTimeFormat = "dddd, MMMM dd, yyyy  hh:mm:ss tt";

                            foreach (UnicastIPAddressInformation uni in uniCast)
                            {
                                DateTime when;

                                // Format the lifetimes as Sunday, February 16, 2003 11:33:44 PM
                                // if en-us is the current culture.

                                // Calculate the date and time at the end of the lifetimes.

                                //when = DateTime.UtcNow + TimeSpan.FromSeconds(uni.AddressValidLifetime);
                                //when = when.ToLocalTime();
                                //s += "     Valid Life Time ...................... : " + when.ToString(lifeTimeFormat, System.Globalization.CultureInfo.CurrentCulture) + "\n";

                                //when = DateTime.UtcNow + TimeSpan.FromSeconds(uni.AddressPreferredLifetime);
                                //when = when.ToLocalTime();
                                //s += "     Preferred life time .................. : " + when.ToString(lifeTimeFormat, System.Globalization.CultureInfo.CurrentCulture) + "\n";

                                when = DateTime.UtcNow + TimeSpan.FromSeconds(uni.DhcpLeaseLifetime);
                                //s += "utc now " + when.ToString() + "\n";

                                when = when.ToLocalTime();
                                //s += "local time " + when.ToString() + "\n";

                                //s += "     DHCP Leased Life Time ................ : " + when.ToString(lifeTimeFormat, System.Globalization.CultureInfo.CurrentCulture) + "\n";

                                if (uni.DhcpLeaseLifetime > 0 && when > localDate)
                                {
                                    dhcpEnabled = true;
                                    return dhcpEnabled;
                                }
                                //return s;
                                s += "\n";
                                //break;  //only 1st adapter (Ethernet) matters, others are virtual machines
                            }

                        }
                        //break; //only 1st adapter (Ethernet) matters, others are virtual machines
                    }

                }

            }
            //s += " Le DHCP n'est pas fonctionnel sur ce PC\n";
            dhcpEnabled = false;
            return dhcpEnabled;
        }

        static bool erreur = false;

        public static string ShowIPAddresses()
        {
            string s = "";
            //bool dhcpEnabled;
            DateTime localDate = DateTime.Now;           

            //Task t = Task.Run(() =>
            //{
            //    Task.Delay(5000).Wait();
            //});

                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            //foreach (NetworkInterface adapter in nics)
            //{
            //    s += adapter.Description + "\n";
            //}
            try
            {
                // for all network interfaces
                foreach (NetworkInterface adapter in nics)
                {
                    //s += nics[i].Description + "\n";

                    // get only Ethernet and active adapters (VM included)
                    if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet && adapter.OperationalStatus == OperationalStatus.Up)
                    {
                        //s += "okokokokokokok\n";

                        //get its properties
                        IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                        IPv4InterfaceProperties p = adapterProperties.GetIPv4Properties();

                        // Network interface description
                        //s += nic0.Description + "\n";

                        // Check if its DHCP is enabled
                        if (p.IsDhcpEnabled)
                        {
                            //get its properties
                            UnicastIPAddressInformationCollection uniCast = adapterProperties.UnicastAddresses;
                            GatewayIPAddressInformationCollection addresses = adapterProperties.GatewayAddresses;
                            GatewayIPAddressInformation address0 = addresses[0];
                            //s += nic0.Description + " DHCP activé pour cette carte " + p.IsDhcpEnabled + "\n";
                            IPAddress ipDestination = new IPAddress(0x0); // route par défaut 

                            if (GetGatewayForDestination(ipDestination) == addresses[0].Address.ToString()) //checks if NIC uses default route for Internet 
                            {
                                //s += "ipDestination = " + GetGatewayForDestination(ipDestination) + "\n";
                                //s += "passerelle par défaut : " + addresses[0].Address.ToString() + "\n\n";

                                string lifeTimeFormat = "dddd, MMMM dd, yyyy  hh:mm:ss tt";

                                foreach (UnicastIPAddressInformation uni in uniCast)
                                {
                                    DateTime when;

                                    // Format the lifetimes as Sunday, February 16, 2003 11:33:44 PM
                                    // if en-us is the current culture.

                                    // Calculate the date and time at the end of the lifetimes.

                                    //when = DateTime.UtcNow + TimeSpan.FromSeconds(uni.AddressValidLifetime);
                                    //when = when.ToLocalTime();
                                    //s += "     Valid Life Time ...................... : " + when.ToString(lifeTimeFormat, System.Globalization.CultureInfo.CurrentCulture) + "\n";

                                    //when = DateTime.UtcNow + TimeSpan.FromSeconds(uni.AddressPreferredLifetime);
                                    //when = when.ToLocalTime();
                                    //s += "     Preferred life time .................. : " + when.ToString(lifeTimeFormat, System.Globalization.CultureInfo.CurrentCulture) + "\n";

                                    when = DateTime.UtcNow + TimeSpan.FromSeconds(uni.DhcpLeaseLifetime);
                                    //s += "utc now " + when.ToString() + "\n";

                                    when = when.ToLocalTime();
                                    //s += "local time " + when.ToString() + "\n";

                                    //s += "     DHCP Leased Life Time ................ : " + when.ToString(lifeTimeFormat, System.Globalization.CultureInfo.CurrentCulture) + "\n";

                                    if (uni.DhcpLeaseLifetime > 0 && when > localDate)
                                    {
                                        s += "XXXX est compatible\n";
                                        return s;
                                    }

                                    s += "\n";
                                    //break;  //only 1st adapter (Ethernet) matters, others are virtual machines
                                }

                            }
                            //break; //only 1st adapter (Ethernet) matters, others are virtual machines
                        }

                    }
                }
                s += "Pour accéder aux services Citypassenger, contacter le support technique\n";
                return s;
            }
            catch(Exception ex)
            {
                erreur = true; 
                s = "Un problème est survenu lors du lancement de l'application";
                return s;
            }
        }
        //public static bool DhcpEnabled()
        //{
        //    //bool[] dhcpState;
        //    int i = 0;
        //    NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

        //    // for all network interfaces
        //    for (i = 0; i < nics.Length; i++)
        //    {
        //        // get only Ethernet and active adapters
        //        if (nics[i].NetworkInterfaceType == NetworkInterfaceType.Ethernet && nics[i].OperationalStatus == OperationalStatus.Up)
        //        {
        //            NetworkInterface nic0 = nics[i];
        //            IPInterfaceProperties adapterProperties = nic0.GetIPProperties();
        //            IPv4InterfaceProperties p = adapterProperties.GetIPv4Properties();
        //            return p.IsDhcpEnabled;
        //        }
        //    }
        //    return false;
        //    //return dhcpState[0]; // get only 1st interface DHCP state which should be the Ethernet wired connection instead of VM
        //}


    }
}

namespace Wpf_Detect_DHCP
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    /// 
    
    public partial class MainWindow : Window
    {
        private bool _show_image;

        public bool ShowButton
        {
            get { return _show_image; }
        }
        public MainWindow()
        {
            //string var = NetDiag.Program.GetPhysical();
           
            string var = NetDiag.Program.ShowIPAddresses();
            InitializeComponent();
            _show_image = false;
            displayStr(var);

        }

        //private void getImg()
        //{
        //    Image img = new Image();
        //    if (NetDiag.Program.ShowIPAddressesBool())
        //    {
        //        imagePath = "assets/check_ok1.svg";
        //    }
        //    else
        //    {
        //        imagePath = "assets/croix_erreur.png";
        //    }
        //}
        
        
        private void displayStr(string s)
        {
            rapport_dhcp.Text = s;
            if (NetDiag.Program.ShowIPAddressesBool())
            {
                txtBeforeUrl.Text = "Voir les produits Citypassenger sur ";
                urlText.Text = "Voir nos produits";
                
            }
            else
            {
                txtBeforeUrl.Text = "";
                urlText.Text = "Contacter le support technique de Citypassenger";
            }
        }

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    Process p = new Process();
        //    p.StartInfo.UseShellExecute = true;
        //    if (NetDiag.Program.ShowIPAddressesBool()) // if DHCP enabled
        //    {
        //        p.StartInfo.FileName = "https://www.citypassenger.com/dhcpok";
        //        p.Start();
        //    }
        //    else // if DHCP disabled
        //    {
        //        p.StartInfo.FileName = "https://www.citypassenger.com/dhcpok";
        //        p.Start();
        //    }
        //}

        private void Hyperlink_RequestNavigate(object sender,
                                       System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = true;
            _show_image = NetDiag.Program.ShowIPAddressesBool();
            if (NetDiag.Program.ShowIPAddressesBool()) // if DHCP enabled
            {
                //image1.Visibility = Visibility.Visible;
                //System.Windows.Visibility vis = new System.Windows.Visibility();
                //image1.Visibility = "Hidden";
                p.StartInfo.FileName = "https://www.citypassenger.com/dhcpok";
                p.Start();
            }
            else // if DHCP disabled
            {
                p.StartInfo.FileName = "https://www.citypassenger.com/dhcpfail";
                p.Start();
            }
        }
    }
}


