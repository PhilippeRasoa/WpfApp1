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

namespace NetDiag
{
    class Program
    {
        public static bool DhcpEnabled()
        {
            bool dhcpState;
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addr = ipEntry.AddressList;

            IPInterfaceProperties adapterProperties = nics[0].GetIPProperties();
            IPv4InterfaceProperties p = adapterProperties.GetIPv4Properties();
            dhcpState = p.IsDhcpEnabled;
            //dhcpState = false;

            return dhcpState;
        }

    // Show only IPv4 interfaces. IPv6 not handled
    public static string DisplayIPv4NetworkInterfaces()
        {
            string textBox = "";
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            //Console.WriteLine("IPv4 interface information for {0}.{1}",
            //properties.HostName, properties.DomainName);
            textBox += "IPv4 interface information for " + properties.HostName + "." + properties.DomainName + "\n";
            //Console.WriteLine();

            String strHostName = string.Empty;
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addr = ipEntry.AddressList;

            if (addr.Length == 0)
            {
                textBox += "Pas d'adresse IPv4 détectées";
            }

            foreach (NetworkInterface adapter in nics)
            {
                IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                IPAddressCollection dnsServers = adapterProperties.DnsAddresses;
                //IPAddressInformation ipAddress;

                GatewayIPAddressInformationCollection gwAddresses = adapterProperties.GatewayAddresses;
                UnicastIPAddressInformationCollection uniCast = adapterProperties.UnicastAddresses;
                IPv4InterfaceProperties p = adapterProperties.GetIPv4Properties();
                IPAddressCollection dhcpAddresses = adapterProperties.DhcpServerAddresses;
               
                for (int i = 1; i < addr.Length; i++) //addr = [IPv6, IPv4, IPv6, IPv4,...]
                {
                    foreach (IPAddress dhcpAddress in dhcpAddresses)
                    {
                        foreach (GatewayIPAddressInformation gwAddress in gwAddresses)
                        {
                            foreach (UnicastIPAddressInformation uni in uniCast)
                            {
                                // Only display informatin for interfaces that support IPv4.
                                if (adapter.Supports(NetworkInterfaceComponent.IPv4) == false)
                                {
                                    continue;
                                }
                                //Console.WriteLine(adapter.Description);
                                textBox += adapter.Description + "\n";

                                // Underline the description.
                                //Console.WriteLine(String.Empty.PadLeft(adapter.Description.Length, '='));
                                textBox += String.Empty.PadLeft(adapter.Description.Length, '=') + "\n";

                                if (p == null)
                                {
                                    //Console.WriteLine("No IPv4 information is available for this interface.");
                                    textBox += "No IPv4 information is available for this interface.\n";
                                    //Console.WriteLine();
                                    continue;
                                }
                                // string myIP = Dns.GetHostEntry(hostName).AddressList[1].ToString();
                                // Console.WriteLine("  Adresse IPv4 :" + myIP);

                                //Console.WriteLine("  DHCP activé ............................. : {0}", p.IsDhcpEnabled);
                                textBox += "  DHCP activé ............................. : " + p.IsDhcpEnabled + "\n";

                                if (dhcpAddresses.Count > 0)
                                {

                                    //Console.WriteLine("  IP Adresse ............................ : {0} ", addr[i].ToString());
                                    textBox += "  IP Adresse ............................ : " + addr[i].ToString() + "\n";



                                    //Console.WriteLine("  Serveur DHCP ............................ : {0}", address.ToString());
                                    textBox += "  Serveur DHCP ............................ : " + dhcpAddress.ToString() + "\n";



                                    //Console.WriteLine("  Passerelle par défaut ......................... : {0}", address.Address.ToString());
                                    textBox += "  Passerelle par défaut ......................... : " + gwAddress.Address.ToString() + "\n";


                                }
                                if (uniCast != null)
                                {
                                    string lifeTimeFormat = "dddd, MMMM dd, yyyy  hh:mm:ss tt";

                                    DateTime when;

                                    //Console.WriteLine("  Unicast Address ......................... : {0}", uni.Address);
                                    //textBox += "  Unicast Address ......................... : " + uni.Address + "\n";

                                    when = DateTime.UtcNow + TimeSpan.FromSeconds(uni.AddressValidLifetime);
                                    when = when.ToLocalTime();
                                    //Console.WriteLine("  Durée de vie valide ...................... : {0}", when.ToString(lifeTimeFormat, System.Globalization.CultureInfo.CurrentCulture));
                                    textBox += "  Durée de vie valide ...................... : " + when.ToString(lifeTimeFormat, System.Globalization.CultureInfo.CurrentCulture) + "\n";

                                    //when = DateTime.UtcNow + TimeSpan.FromSeconds(uni.AddressPreferredLifetime);
                                    //when = when.ToLocalTime();
                                    ////Console.WriteLine("     Preferred life time .................. : {0}", when.ToString(lifeTimeFormat, System.Globalization.CultureInfo.CurrentCulture));
                                    //textBox += "  Durée de vie préférée .................. : " + when.ToString(lifeTimeFormat, System.Globalization.CultureInfo.CurrentCulture) + "\n";

                                    when = DateTime.UtcNow + TimeSpan.FromSeconds(uni.DhcpLeaseLifetime);
                                    when = when.ToLocalTime();
                                    //Console.WriteLine("  Durée de vie du bail DHCP ................ : {0}", when.ToString(lifeTimeFormat, System.Globalization.CultureInfo.CurrentCulture));
                                    textBox += "  Durée de vie du bail DHCP ................ : " + when.ToString(lifeTimeFormat, System.Globalization.CultureInfo.CurrentCulture) + "\n";
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            //Console.WriteLine(textBox);
            return textBox;
        }
    }
}



namespace Wpf_Detect_DHCP
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            string var = NetDiag.Program.DisplayIPv4NetworkInterfaces();
            InitializeComponent();
            displayStr(var);
        }
        private void displayStr(string s)
        {
            rapport_dhcp.Text = s;
            if (NetDiag.Program.DhcpEnabled()) // if DHCP enabled
            {
                button_name.Content = "DHCP activé. Cliquer pour un diagnostique";
            }
            else // if DHCP disabled
            {
                button_name.Content = "DHCP désactivé. Cliquer pour un diagnostique";
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (NetDiag.Program.DhcpEnabled()) // if DHCP enabled
            {
                System.Diagnostics.Process.Start("https://www.citypassenger.com/dhcpok");
            }
            else // if DHCP disabled
            {
                System.Diagnostics.Process.Start("https://www.citypassenger.com/dhcpfail");
            }
        }
    }  
}


