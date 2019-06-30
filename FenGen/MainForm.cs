﻿using System;
using System.Windows.Forms;

namespace FenGen
{
    internal partial class MainForm : Form
    {
        internal Model Model { get; set; }

        internal MainForm()
        {
            InitializeComponent();
        }

        private void GenerateButton_Click(object sender, EventArgs e)
        {
            Model.Generate();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}