﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace wowiebot
{
    public partial class ConfigForm : Form
    {
        DataSet commandsDataSet;

        public ConfigForm()
        {
            InitializeComponent();
            discordTextBox.Enabled = discordCheckBox.Checked;
            prefixTextBox.Text = Properties.Settings.Default.prefix;
            quoteCheckBox.Checked = Properties.Settings.Default.enableQuotes;
            editQuotesButton.Enabled = Properties.Settings.Default.enableQuotes;
            uptimeCheckBox.Checked = Properties.Settings.Default.enableUptime;
            titleCheckBox.Checked = Properties.Settings.Default.enableTitle;
            linkCheckBox.Checked = Properties.Settings.Default.enableLinkTitles;
            discordCheckBox.Checked = Properties.Settings.Default.enableDiscord;
            eightBallCheckBox.Checked = Properties.Settings.Default.enable8Ball;
            if (Properties.Settings.Default.enableDiscord)
            {
                discordTextBox.Text = Properties.Settings.Default.discordServer;
            }
            else
            {
                discordTextBox.Text = "https://";
            }
            updateSaveButton();
            commandsDataSet = new DataSet();
            DataTable commandsDataTable = new DataTable();
            DataColumn cmd = new DataColumn("Command");
            DataColumn fn = new DataColumn("Function");
            DataColumn parm = new DataColumn("Parameters");
            commandsDataTable.Columns.Add(cmd);
            commandsDataTable.Columns.Add(fn);
            commandsDataTable.Columns.Add(parm);
            commandsDataSet.Tables.Add(commandsDataTable);
            //this.commandsDataTable = Properties.Settings.Default.commandsDataSet;
            dataGridView1.DataSource = commandsDataSet;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.prefix = prefixTextBox.Text;
            Properties.Settings.Default.enableQuotes = quoteCheckBox.Checked;
            Properties.Settings.Default.enableUptime = uptimeCheckBox.Checked;
            Properties.Settings.Default.enableTitle = titleCheckBox.Checked;
            Properties.Settings.Default.enableLinkTitles = linkCheckBox.Checked;
            Properties.Settings.Default.enableDiscord = discordCheckBox.Checked;
            Properties.Settings.Default.enable8Ball = eightBallCheckBox.Checked;
            if (discordCheckBox.Checked)
            {
                Properties.Settings.Default.discordServer = discordTextBox.Text;
            }
            string test = JsonConvert.SerializeObject(dataGridView1.DataSource);
            Properties.Settings.Default.commandsDataSetJson = JsonConvert.SerializeObject(dataGridView1.DataSource);
            Properties.Settings.Default.Save();
            Close();
        }

        private void prefixTextBox_TextChanged(object sender, EventArgs e)
        {
            updateSaveButton();
        }

        private void discordCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            discordTextBox.Enabled = discordCheckBox.Checked;
            updateSaveButton();
        }

        private void discordTextBox_TextChanged(object sender, EventArgs e)
        {
            updateSaveButton();
        }

        private void updateSaveButton()
        {
            if (prefixTextBox.Text.Length == 0
                || (discordCheckBox.Checked && discordTextBox.Text == ""))
            {
                saveButton.Enabled = false;
            }
            else
            {
                saveButton.Enabled = true;
            }
        }

        private void quoteCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            editQuotesButton.Enabled = quoteCheckBox.Checked;
        }

        private void editQuotesButton_Click(object sender, EventArgs e)
        {
            EditStringsForm quotesForm = new EditStringsForm("quotes");
            quotesForm.StartPosition = FormStartPosition.CenterScreen;
            quotesForm.ShowDialog();
        }

        private void edit8BallChoicesButton_Click(object sender, EventArgs e)
        {
            EditStringsForm quotesForm = new EditStringsForm("choices");
            quotesForm.StartPosition = FormStartPosition.CenterScreen;
            quotesForm.ShowDialog();
        }
    }
}
