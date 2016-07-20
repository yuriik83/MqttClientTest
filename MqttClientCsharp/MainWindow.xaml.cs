using System;
using System.Text;
using System.Windows;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Threading;

namespace MqttClientCsharp
{
    /// <summary>
    /// MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MqttClient client;
        StringBuilder sb = new StringBuilder(4096);
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            //handle messge received
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                (ThreadStart)delegate()
                    {
                        if (sb.Length > 10000)
                        {
                            sb.Clear();
                        }
                        sb.Append("Topic:" + e.Topic.ToString() + " Len:" + e.Message.Length.ToString() + " Msg:");
                        if (hexCheckBox.IsChecked == true)
                        {
                            foreach (byte b in e.Message)
                            {
                                sb.Append(b.ToString("x2").ToUpper() + " ");
                            }
                        }
                        else
                        {
                            sb.Append(Encoding.Default.GetString(e.Message));
                        }
                        sb.AppendLine();
                        recvInfoTB.Text = sb.ToString();
                    }
                );
        }

        private void publishBtn_Click(object sender, RoutedEventArgs e)
        {
            if (client != null && client.IsConnected)
            {
                try
                {
                    if (pubTopicTB.Text.Length != 0)
                    {
                        client.Publish(pubTopicTB.Text, Encoding.Default.GetBytes(pubInfoTB.Text), (byte)subQosCB.SelectedIndex, false);
                    }
                    else
                    {
                        MessageBox.Show("Please enter a valid topic");
                    }

                }
                catch (Exception ex)
                {
                    statusTB.Text = ex.Message;
                }
            }
            else
            {
                MessageBox.Show("not connected Broker");
            }
        }

        private void subBtn_Click(object sender, RoutedEventArgs e)
        {
            if (client != null && client.IsConnected)
            {
                if (topicTB.Text.Length != 0)
                {
                    MqttSubscribeTopic(topicTB.Text, (byte)qosCB.SelectedIndex);
                }
                else
                {
                    MessageBox.Show("Please enter a valid topic");
                }
            }
            else
            {
                MessageBox.Show("not connected Broker");
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="qoslevel"></param>
        private void MqttSubscribeTopic(string topic, byte qoslevel)
        {

            string[] topics = { topic };
            byte[] qoss = { qoslevel };
            try
            { client.Subscribe(topics, qoss); }
            catch (Exception ex)
            { statusTB.Text = ex.Message; }

        }

        private void connectBtn_Click(object sender, RoutedEventArgs e)
        {
            string clientId = Guid.NewGuid().ToString();
            try
            {
                client = new MqttClient(brokerTB.Text);
                client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
                client.Connect(clientId);
                statusTB.Text = "Successfully connected to Broker";
            }
            catch (Exception ex)
            {
                statusTB.Text = ex.Message;
            }
        }

        private void disconnectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (client != null && client.IsConnected)
            {
                client.Disconnect();
                statusTB.Text = "Broker not connected";
            }
        }

    }
}
