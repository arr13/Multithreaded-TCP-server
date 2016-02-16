using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace Client
{
    public partial class MainWindow : Window
    {
        private int countClickBtnSave;
        private int countClickBtnEncrypt;
        private int countClickBtnDecrypt;
        private int countClickBtnLogIn;
        private TcpClient connection;
        NetworkStream stream;
        private BinaryWriter writer;
        private BinaryReader reader;

        public const string IP_ADDRESS = "127.0.0.1";
        public const int PORT_NUMBER = 50000;

        public MainWindow()
        {
            InitializeComponent();

            countClickBtnSave = 0;
            countClickBtnDecrypt = 0;
            countClickBtnEncrypt = 0;
            countClickBtnLogIn = 0;

            passwordBox.Visibility = System.Windows.Visibility.Hidden;
            txtBoxAnswerCypher.Visibility = System.Windows.Visibility.Hidden;
            txtBoxUserName.Visibility = System.Windows.Visibility.Hidden;
            txtBoxCardNo.Visibility = System.Windows.Visibility.Hidden;
            checkBoxDecrypt.Visibility = System.Windows.Visibility.Hidden;
            checkBoxEncrypt.Visibility = System.Windows.Visibility.Hidden;
            lblPassword.Visibility = System.Windows.Visibility.Hidden;
            lblUseName.Visibility = System.Windows.Visibility.Hidden;
            btnDecrypt.Visibility = System.Windows.Visibility.Hidden;
            btnEncrypt.Visibility = System.Windows.Visibility.Hidden;

            connection = new TcpClient(IP_ADDRESS, PORT_NUMBER); // create TcpClient
            stream = connection.GetStream(); // create NetworkStream object
            writer = new BinaryWriter(stream); // create BinaryWriter object
            reader = new BinaryReader(stream); // create BinaryReader object
        }

        // implements save request
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (countClickBtnSave == 0)
            {
                countClickBtnSave = 1;

                passwordBox.Visibility = System.Windows.Visibility.Visible;
                txtBoxAnswerCypher.Visibility = System.Windows.Visibility.Hidden;
                txtBoxUserName.Visibility = System.Windows.Visibility.Visible;
                txtBoxCardNo.Visibility = System.Windows.Visibility.Hidden;
                checkBoxDecrypt.Visibility = System.Windows.Visibility.Visible;
                checkBoxEncrypt.Visibility = System.Windows.Visibility.Visible;
                lblPassword.Visibility = System.Windows.Visibility.Visible;
                lblUseName.Visibility = System.Windows.Visibility.Visible;
                btnLogin.Visibility = System.Windows.Visibility.Hidden;
                
                writer.Write("save");
            }
            else if (!passwordBox.Password.Equals("") && !txtBoxUserName.Text.Equals("") &&
                (checkBoxDecrypt.IsChecked == true || checkBoxEncrypt.IsChecked == true))
            {
                string userName = txtBoxUserName.Text;
                Regex regexUserName = new Regex("^(?=.{8,20}$)(?![_.])(?!.*[_.]{2})[a-zA-Z0-9._]+(?<![_.])$");
                Match matcherUserName = regexUserName.Match(userName);

                string password = passwordBox.Password;
                Regex regexPassword = new Regex("^((?=.*\\d)(?=.*[a-zA-Z])[a-zA-Z0-9!@#$%&*]{6,20})$");
                Match matcherPassword = regexPassword.Match(password);

                if (matcherUserName.Success == true && matcherPassword.Success == true)
                {
                    writer.Write(userName);
                    writer.Write(password);
                    writer.Write(checkBoxEncrypt.IsChecked == true);
                    writer.Write(checkBoxDecrypt.IsChecked == true);

                    string replyToRequest = reader.ReadString();

                    if (replyToRequest.Equals("alreadyExists"))
                    {
                        MessageBox.Show("User with this user name already exists!");
                        passwordBox.Password = "";
                        txtBoxUserName.Text = "";

                        txtBoxAnswerCypher.Visibility = System.Windows.Visibility.Hidden;
                        passwordBox.Visibility = System.Windows.Visibility.Hidden;
                        txtBoxUserName.Visibility = System.Windows.Visibility.Hidden;
                        txtBoxCardNo.Visibility = System.Windows.Visibility.Hidden;
                        checkBoxDecrypt.Visibility = System.Windows.Visibility.Hidden;
                        checkBoxEncrypt.Visibility = System.Windows.Visibility.Hidden;
                        lblPassword.Visibility = System.Windows.Visibility.Hidden;
                        lblUseName.Visibility = System.Windows.Visibility.Hidden;
                        passwordBox.Password = "";
                        txtBoxUserName.Text = "";
                        btnLogin.Visibility = System.Windows.Visibility.Visible;

                        countClickBtnSave = 0;
                        countClickBtnLogIn = 0;
                    }
                    else if (replyToRequest.Equals("added"))
                    {
                        MessageBox.Show("User added successfully!");

                        txtBoxAnswerCypher.Visibility = System.Windows.Visibility.Hidden;
                        passwordBox.Visibility = System.Windows.Visibility.Hidden;
                        txtBoxUserName.Visibility = System.Windows.Visibility.Hidden;
                        txtBoxCardNo.Visibility = System.Windows.Visibility.Hidden;
                        checkBoxDecrypt.Visibility = System.Windows.Visibility.Hidden;
                        checkBoxEncrypt.Visibility = System.Windows.Visibility.Hidden;
                        lblPassword.Visibility = System.Windows.Visibility.Hidden;
                        lblUseName.Visibility = System.Windows.Visibility.Hidden;
                        passwordBox.Password = "";
                        txtBoxUserName.Text = "";
                        btnLogin.Visibility = System.Windows.Visibility.Visible;

                        countClickBtnLogIn = 0;
                        countClickBtnSave = 0;
                    }
                }
                else
                {
                    MessageBox.Show("Incorrect input!");
                    txtBoxUserName.Text = "";
                    passwordBox.Password = "";
                }
            }
            else
            {
                MessageBox.Show("Not all fields are filled!");
                passwordBox.Password = "";
            }
        }

        // implements login request
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (countClickBtnLogIn == 0)
            {
                txtBoxAnswerCypher.Visibility = System.Windows.Visibility.Hidden;
                passwordBox.Visibility = System.Windows.Visibility.Visible;
                txtBoxUserName.Visibility = System.Windows.Visibility.Visible;
                txtBoxCardNo.Visibility = System.Windows.Visibility.Hidden;
                checkBoxDecrypt.Visibility = System.Windows.Visibility.Hidden;
                checkBoxEncrypt.Visibility = System.Windows.Visibility.Hidden;
                lblPassword.Visibility = System.Windows.Visibility.Visible;
                lblUseName.Visibility = System.Windows.Visibility.Visible;
                btnSave.Visibility = System.Windows.Visibility.Hidden;

                writer.Write("logIn");

                countClickBtnLogIn = 1;
            }
            else
            {
                if (!passwordBox.Password.Equals("") && !txtBoxUserName.Text.Equals(""))
                {
                    string userName = txtBoxUserName.Text;
                    Regex regexUserName = new Regex("^(?=.{8,20}$)(?![_.])(?!.*[_.]{2})[a-zA-Z0-9._]+(?<![_.])$");
                    Match matcherUserName = regexUserName.Match(userName);

                    string password = passwordBox.Password;
                    Regex regexPassword = new Regex("^((?=.*\\d)(?=.*[a-zA-Z])[a-zA-Z0-9!@#$%&*]{6,20})$");
                    Match matcherPassword = regexPassword.Match(password);

                    if (matcherUserName.Success == true && matcherPassword.Success == true)
                    {
                        writer.Write(userName);
                        writer.Write(password);

                        string replyToRequest = reader.ReadString();

                        if (replyToRequest.Equals("notLoggedIn"))
                        {
                            MessageBox.Show("Such user doesn`t exist");
                            passwordBox.Password = "";
                            txtBoxUserName.Text = "";

                            countClickBtnSave = 0;
                            countClickBtnLogIn = 0;

                            txtBoxAnswerCypher.Visibility = System.Windows.Visibility.Hidden;
                            passwordBox.Visibility = System.Windows.Visibility.Hidden;
                            txtBoxUserName.Visibility = System.Windows.Visibility.Hidden;
                            txtBoxCardNo.Visibility = System.Windows.Visibility.Hidden;
                            checkBoxDecrypt.Visibility = System.Windows.Visibility.Hidden;
                            checkBoxEncrypt.Visibility = System.Windows.Visibility.Hidden;
                            lblPassword.Visibility = System.Windows.Visibility.Hidden;
                            lblUseName.Visibility = System.Windows.Visibility.Hidden;
                            btnLogin.Visibility = System.Windows.Visibility.Visible;
                            btnSave.Visibility = System.Windows.Visibility.Visible;

                        }
                        else if (replyToRequest.Equals("loggedIn"))
                        {
                            btnLogin.Visibility = System.Windows.Visibility.Hidden;
                            btnSave.Visibility = System.Windows.Visibility.Hidden;
                            btnEncrypt.Visibility = System.Windows.Visibility.Visible;
                            btnDecrypt.Visibility = System.Windows.Visibility.Visible;
                            lblPassword.Visibility = System.Windows.Visibility.Hidden;
                            lblUseName.Visibility = System.Windows.Visibility.Hidden;
                            passwordBox.Visibility = System.Windows.Visibility.Hidden;
                            txtBoxAnswerCypher.Visibility = System.Windows.Visibility.Hidden;
                            txtBoxUserName.Visibility = System.Windows.Visibility.Hidden;
                            txtBoxCardNo.Visibility = System.Windows.Visibility.Hidden;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Incorrect input!");
                        txtBoxUserName.Text = "";
                        passwordBox.Password = "";
                    }
                }
                else
                {
                    MessageBox.Show("Not all fields are filled!");
                    passwordBox.Password = "";
                }
            }
        }

        // implements encrypt request
        private void btnEncrypt_Click(object sender, RoutedEventArgs e)
        {
            if (countClickBtnEncrypt == 0)
            {
                string userName = txtBoxUserName.Text;

                lblPassword.Visibility = System.Windows.Visibility.Hidden;
                lblUseName.Visibility = System.Windows.Visibility.Visible;
                lblUseName.Content = "Card No.";
                passwordBox.Visibility = System.Windows.Visibility.Hidden;
                txtBoxUserName.Visibility = System.Windows.Visibility.Hidden;
                txtBoxAnswerCypher.Visibility = System.Windows.Visibility.Visible;
                txtBoxCardNo.Visibility = System.Windows.Visibility.Visible;
                txtBoxCardNo.Text = "";
                txtBoxAnswerCypher.Text = "";
                txtBoxAnswerCypher.IsReadOnly = true;
                btnDecrypt.Visibility = System.Windows.Visibility.Hidden;
                txtBoxCardNo.IsReadOnly = false;

                countClickBtnEncrypt = 1;

                writer.Write("encrypt");
                writer.Write(userName);
            }
            else
            {
                string reply = reader.ReadString();
         
                if (reply.Equals("allowed"))
                {
                    if (txtBoxCardNo.Text != "")
                    {
                        writer.Write(txtBoxCardNo.Text);
                        string r = reader.ReadString();

                        if (r.Equals("validCardNo"))
                        {
                            string encryption = reader.ReadString();

                            if (encryption.Equals("cannotEncrypt"))
                            {
                                MessageBox.Show("This card is encrypted already 12 times");

                                txtBoxCardNo.Visibility = System.Windows.Visibility.Hidden;
                                txtBoxAnswerCypher.Visibility = System.Windows.Visibility.Hidden;
                                lblUseName.Visibility = System.Windows.Visibility.Hidden;
                            }
                            else
                            {
                                txtBoxAnswerCypher.Text = encryption;
                            }
                        }
                        else if (r.Equals("invalidCardNo"))
                        {
                            MessageBox.Show("Invalid card number");
                        }

                        btnLogin.Visibility = System.Windows.Visibility.Hidden;
                        btnSave.Visibility = System.Windows.Visibility.Hidden;
                        btnEncrypt.Visibility = System.Windows.Visibility.Visible;
                        btnDecrypt.Visibility = System.Windows.Visibility.Visible;
                        lblPassword.Visibility = System.Windows.Visibility.Hidden;
                        txtBoxCardNo.IsReadOnly = false;
                        txtBoxUserName.Visibility = System.Windows.Visibility.Hidden;
                        passwordBox.Visibility = System.Windows.Visibility.Hidden;

                        countClickBtnEncrypt = 0;
                    }
                    else
                    {
                        MessageBox.Show("The field Card No. is empty!");
                    }
                }
                else if (reply.Equals("notAllowed"))
                {
                    MessageBox.Show("This user isn`t allow to encrypt!");

                    countClickBtnLogIn = 0;
                    countClickBtnSave = 0;
                    countClickBtnDecrypt = 0;
                    countClickBtnEncrypt = 0;

                    lblUseName.Visibility = System.Windows.Visibility.Hidden;
                    txtBoxCardNo.Visibility = System.Windows.Visibility.Hidden;
                    txtBoxAnswerCypher.Visibility = System.Windows.Visibility.Hidden;
                    btnDecrypt.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        // implements decrypt request
        private void btnDecrypt_Click(object sender, RoutedEventArgs e)
        {
            if (countClickBtnDecrypt == 0)
            {
                string userName = txtBoxUserName.Text;

                passwordBox.Visibility = System.Windows.Visibility.Hidden;
                lblPassword.Visibility = System.Windows.Visibility.Hidden;
                lblUseName.Visibility = System.Windows.Visibility.Visible;
                lblUseName.Content = "Code";
                txtBoxUserName.Visibility = System.Windows.Visibility.Hidden;
                txtBoxAnswerCypher.Visibility = System.Windows.Visibility.Visible;
                txtBoxCardNo.Visibility = System.Windows.Visibility.Visible;
                txtBoxCardNo.Text = "";
                txtBoxAnswerCypher.Text = "";
                txtBoxAnswerCypher.IsReadOnly = true;
                btnEncrypt.Visibility = System.Windows.Visibility.Hidden;
                txtBoxCardNo.IsReadOnly = false;

                countClickBtnDecrypt = 1;

                writer.Write("decrypt");
                writer.Write(userName);
            }
            else
            {
                string reply = reader.ReadString();

                if (reply.Equals("allowed"))
                {
                    if (txtBoxCardNo.Text != "")
                    {
                        writer.Write(txtBoxCardNo.Text);
                        string result = reader.ReadString();
                        txtBoxAnswerCypher.Text = result;

                        btnLogin.Visibility = System.Windows.Visibility.Hidden;
                        btnSave.Visibility = System.Windows.Visibility.Hidden;
                        btnEncrypt.Visibility = System.Windows.Visibility.Visible;
                        btnDecrypt.Visibility = System.Windows.Visibility.Visible;
                        lblPassword.Visibility = System.Windows.Visibility.Hidden;
                        txtBoxCardNo.IsReadOnly = false;
                        txtBoxUserName.Visibility = System.Windows.Visibility.Hidden;

                        countClickBtnDecrypt = 0;
                    }
                    else
                    {
                        MessageBox.Show("The field Code is empty!");
                    }
                }
                else if (reply.Equals("notAllowed"))
                {
                    MessageBox.Show("This user isn`t allowed to decrypt!");

                    countClickBtnLogIn = 0;
                    countClickBtnSave = 0;
                    countClickBtnDecrypt = 0;
                    countClickBtnEncrypt = 0;
                }
            }
        }

        // terminate program and close connection with the server
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            connection.Close();
        }
    }
}