using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Microsoft.VisualBasic;

namespace ChatApp
{
    public partial class Form1 : Form
    {
        string ConnectString = "server=172.16.2.26;database=chat_group5;user=1;password=1;charset=utf8mb4";

        string currentUserName = "";
        List<int> msgIds = new List<int>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (PerformLogin())
            {
                timer1.Interval = 1000;
                timer1.Start();
                this.Text = "Chat App Group5 - Welcome: " + currentUserName;
            }
            else
            {
                Application.Exit();
            }
        }

        private bool PerformLogin()
        {
            string user = Interaction.InputBox("Please enter your username", "Login", "");
            string pass = Interaction.InputBox("Please enter your password", "Login", "");

            using (MySqlConnection conn = new MySqlConnection(ConnectString))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT COUNT(*) FROM users WHERE username=@u AND password=@p";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@u", user);
                    cmd.Parameters.AddWithValue("@p", pass);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    if (count > 0)
                    {
                        currentUserName = user;
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Incorrect username or password!");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to connect to the database: " + ex.Message);
                    return false;
                }
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInput.Text))
                return;

            using (MySqlConnection conn = new MySqlConnection(ConnectString))
            {
                try
                {
                    conn.Open();
                    string sql = "INSERT INTO messages (sender_name, content) VALUES (@name, @msg)";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@name", currentUserName);
                    cmd.Parameters.AddWithValue("@msg", txtInput.Text);

                    cmd.ExecuteNonQuery();
                    txtInput.Clear();
                    txtInput.Focus();
                }
                catch (Exception ex) { MessageBox.Show("Failed to send: " + ex.Message); }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            using (MySqlConnection conn = new MySqlConnection(ConnectString))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT id, sender_name, content, created_at FROM messages WHERE is_deleted = 0 ORDER BY id ASC";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        int savedIndex = lstMessages.SelectedIndex;
                        lstMessages.Items.Clear();
                        msgIds.Clear();

                        while (reader.Read())
                        {
                            int id = Convert.ToInt32(reader["id"]);
                            string name = reader["sender_name"].ToString();
                            string msg = reader["content"].ToString();
                            string time = Convert.ToDateTime(reader["created_at"]).ToString("HH:mm:ss");

                            lstMessages.Items.Add($"[{time}] {name}: {msg}");
                            msgIds.Add(id);
                        }

                        if (savedIndex != -1 && savedIndex < lstMessages.Items.Count)
                            lstMessages.SelectedIndex = savedIndex;
                    }
                }
                catch { }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lstMessages.SelectedIndex == -1)
                return;
            int targetId = msgIds[lstMessages.SelectedIndex];

            using (MySqlConnection conn = new MySqlConnection(ConnectString))
            {
                try
                {
                    conn.Open();
                    string sql = "UPDATE messages SET is_deleted = 1 WHERE id = @id";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@id", targetId);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex) { MessageBox.Show("Failed to delete: " + ex.Message); }
            }
        }
    }
}