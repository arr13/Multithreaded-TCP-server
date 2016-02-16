using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

namespace Server
{
    public partial class MainWindow : Window
    {
        // constants
        public const string IP_ADDRESS = "127.0.0.1";
        public const int PORT_NUMBER = 50000;

        public List<User> users; 
        public List<CreditCard> cards;
        private Thread readThread; // Thread for processing incoming messages

        public MainWindow()
        {
            InitializeComponent();
            Application.Current.MainWindow.Closing += new System.ComponentModel.CancelEventHandler(MainWindow_Closing); // add a delegate that handles the closing of the form

            users = new List<User>();
            cards = new List<CreditCard>();

            // start thread for incoming messages
            readThread = new Thread(new ThreadStart(RunServer));
            readThread.Start();
        }

        // terminate program and close all its threads
        public void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Environment.Exit(System.Environment.ExitCode);
        }

        // delegate that allows the method DisplayMessage to be called
        // in the thread thatcreates and maintains the GUI
        public delegate void DisplayDelegate(string message);


        public void DisplayMessage(string message)
        {
            // if modifying txtBoxDisplay is not thread safe
            if (txtBoxDisplay.Dispatcher.CheckAccess())
            {
                txtBoxDisplay.Text += message;
            }
            else
            {
                txtBoxDisplay.Dispatcher.Invoke(new DisplayDelegate(DisplayMessage), new object[] { message });
            }
        }

        // implement what server does
        public void RunServer()
        {
            Socket socket; // accept a connection
            TcpListener listener; // create TcpListener
            int counter = 1;

            try
            {
                IPAddress local = IPAddress.Parse(IP_ADDRESS);

                listener = new TcpListener(local, PORT_NUMBER);
                listener.Start(); // // TcpListener waits for connection request
                DisplayMessage(String.Format("Server Started IP={0} on Port: {1}\n", IP_ADDRESS, PORT_NUMBER));
                DisplayMessage("Listening for client connections...\n");

                while (true)
                {
                    socket = listener.AcceptSocket();// accept an incoming connection
                    DisplayMessage(String.Format("\n>> Connection " + Convert.ToString(counter) + " started\n"));
                    ThreadPool.QueueUserWorkItem(new WaitCallback(HandleClient), socket); // create new thread

                    counter++;
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }

        // handle each client separately
        private void HandleClient(object socket)
        {
            Socket connection = (Socket)socket;
            NetworkStream socketStream = new NetworkStream(connection);
            BinaryWriter writer = new BinaryWriter(socketStream);
            BinaryReader reader = new BinaryReader(socketStream);

            string request = string.Empty;

            do
            {
                try
                {
                    request = reader.ReadString();
                    // implements request save
                    if (request.Equals("save"))
                    {
                        DisplayMessage("Request is SAVE\n");
                        string userName = reader.ReadString();
                        string password = reader.ReadString();
                        bool canUserEncrypt = reader.ReadBoolean();
                        bool canUserDecrypt = reader.ReadBoolean();

                        if (SaveUser(userName, password, canUserEncrypt, canUserDecrypt) == false)
                        {
                            DisplayMessage("User " + userName + " added\n");
                            writer.Write("added");
                            continue;
                        }
                        else
                        {
                            writer.Write("alreadyExists");
                            DisplayMessage("This user already exists!\n");
                            continue;
                        }
                    }
                    // implements request login
                    else if (request.Equals("logIn"))
                    {
                        DisplayMessage("Request is LOGIN\n");

                        string userName = reader.ReadString();
                        string password = reader.ReadString();

                        if (LogIn(userName, password) == true)
                        {
                            DisplayMessage("User logged in successfully\n");
                            writer.Write("loggedIn");

                            continue;
                        }
                        else
                        {
                            DisplayMessage("Such user doesn`t exist\n");
                            writer.Write("notLoggedIn");
                            continue;
                        }
                    }
                    // implements request encrypt
                    else if (request.Equals("encrypt"))
                    {
                        DisplayMessage("Request is ENCRYPT\n");

                        string userName = reader.ReadString();

                        User user = findUserByUserName(userName);

                        if (user.CanUserEncrypt == true)
                        {
                            writer.Write("allowed");
                            DisplayMessage(user.UserName + " is allowed to encrypt\n");

                            string cardNo = reader.ReadString();
                            if (IsValidNumber(cardNo) == true)
                            {
                                writer.Write("validCardNo");
                                CreditCard tempCreditCard = IsCreditCardInCreditCardsList(cardNo);

                                if (tempCreditCard == null)
                                {
                                    CreditCard temp = new CreditCard(cardNo);
                                    cards.Add(temp);
                                    writer.Write(temp.Encrypt(cardNo));
                                }
                                else
                                {
                                    writer.Write(tempCreditCard.Encrypt(cardNo));
                                }

                                DisplayMessage("Credit card is encrypted successfully\n");
                            }
                            else
                            {
                                writer.Write("invalidCardNo");
                                DisplayMessage("Invalid card number\n");
                                continue;
                            }
                        }
                        else
                        {
                            writer.Write("notAllowed");
                            DisplayMessage("Encryption for " + user.UserName + " is not allowed\n");

                            continue;
                        }
                    }
                    // implements request decrypt 
                    else if (request.Equals("decrypt"))
                    {
                        DisplayMessage("Request is DECRYPT\n");

                        string userName = reader.ReadString();

                        User user = findUserByUserName(userName);

                        if (user.CanUserDecrypt == true)
                        {
                            writer.Write("allowed");
                            DisplayMessage(user.UserName + " is allowed to decrypt\n");

                            string cypher = reader.ReadString();

                            writer.Write(CreditCard.Decrypt(cypher));

                        }
                        else
                        {
                            writer.Write("notAllowed");
                            DisplayMessage("Decryption for " + user.UserName + " is not allowed\n");

                            continue;
                        }
                    }
                }
                // exceptions
                catch (IOException)
                {
                    if (connection.Connected)
                    {
                        DisplayMessage("The client closed the connection\n");
                        break;
                    }
                    else
                        DisplayMessage("Could not connect. User closed the connection\n");
                }
                catch (Exception)
                {
                    DisplayMessage("Error receiving data from Client\n");
                    break;
                }
            } while (connection.Connected);

            // free resources
            writer.Close();
            reader.Close();
            connection.Close();
            socketStream.Close();
        }

        // method that saves user in list users
        private bool SaveUser(string userName, string password, bool canUserEncrypt, bool canUserDecrypt)
        {
            bool doesItExist = false;

            foreach (var user in users)
            {
                if (userName == user.UserName)
                {
                    doesItExist = true;
                    break;
                }
            }

            if (doesItExist == false)
            {
                this.users.Add(new User(userName, password, canUserEncrypt, canUserDecrypt));
            }

            return doesItExist;
        }

        // checks if user exists
        private bool LogIn(string userName, string password)
        {
            bool doesUserExist = false;

            foreach (var user in users)
            {
                if (user.UserName.Equals(userName) == true && user.Password.Equals(password) == true)
                {
                    doesUserExist = true;
                    break;
                }
            }

            return doesUserExist;
        }

        // finds user by name
        private User findUserByUserName(string userName)
        {
            lock (users)
            {
                foreach (var user in users)
                {
                    if (user.UserName.Equals(userName))
                    {
                        return user;
                    }
                }

                return null;
            }
        }

        // validate credit card number
        private bool IsValidNumber(string number)
        {
            Regex regexNumbers = new Regex("^[0-9]+$");
            Match matchNumbers = regexNumbers.Match(number);

            if (matchNumbers.Success == true)
            {
                int[] DELTAS = new int[] { 0, 1, 2, 3, 4, -4, -3, -2, -1, 0 };
                int checksum = 0;
                char[] chars = number.ToCharArray();
                for (int i = chars.Length - 1; i > -1; i--)
                {
                    int j = ((int)chars[i]) - 48;
                    checksum += j;
                    if (((i - chars.Length) % 2) == 0)
                        checksum += DELTAS[j];
                }

                return ((checksum % 10) == 0);
            }

            return false;
        }

        // checks if credit card is already in the list of credit cards
        private CreditCard IsCreditCardInCreditCardsList(string number)
        {
            lock (cards)
            {
                foreach (var card in cards)
                {
                    if (card.Number.Equals(number))
                    {
                        return card;
                    }
                }

                return null;
            }
        }

        // exports credit cards in txt fiile
        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                XMLSerialization.Export(users, "exportInfo.txt");
            }
            catch (DirectoryNotFoundException ex)
            {
                MessageBox.Show(
                    string.Format("There was a problem exporting these files {0}", ex.Message),
                    string.Format("Error of type {0}", e.GetType()));
            }
            catch (PathTooLongException ex)
            {
                MessageBox.Show(
                    string.Format("There was a problem exporting these files {0}", ex.Message),
                    string.Format("Error of type {0}", e.GetType()));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                        string.Format("there was a problem exporting these files {0}", ex.Message),
                        string.Format("Error of type {0}", e.GetType()));
            }
        }
    }
}
    