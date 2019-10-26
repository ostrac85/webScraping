using BankCardLib.Utility;
using CGB.Models;
using CGB.Models.ConfigSettings;
using CGB.Properties;
using CGB.UAService;
using Newtonsoft.Json;
using System;
using System.Windows.Forms;
using static CGB.Models.ModelData;

namespace CGB
{
    public partial class frmSetParameter : Form
    {
        public frmSetParameter()
        {
            InitializeComponent();
        }

        private async void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(txtCardCode.Text))
                {
                    MessageBox.Show("Please enter Card Code.");
                    return;
                }

                ModelData.Card = BankCardService.GetCard(txtCardCode.Text).Result;

                if (ModelData.Card != null)
                {
                    dgvCards.Rows.Clear();
                    dgvCards.Rows.Add(ModelData.Card.AccountName, ModelData.Card.AccountNumber, "", "", ModelData.Card.Code);
                    btnConfirm.Enabled = true;
                    btnConfirm.Focus();
                }
                else
                {
                    btnConfirm.Enabled = false;
                    dgvCards.Rows.Clear();

                    if (ModelData.Card != null && !(bool)ModelData.Card.Status)
                        MessageBox.Show(WebSetting.Messages.BankCardDisabled, WebSetting.ProgramName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                        MessageBox.Show(WebSetting.Messages.NoBankCards, WebSetting.ProgramName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());

                try
                {
                    await BankCardLib.Services.ExeExceptionsService.SaveExeExeption(new BankCardLib.Models.ExeExceptions() { Bank = WebSetting.ProgramName, Message = ex.Message });
                }
                catch { }
            }
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            this.Hide();
            frmMain main = new frmMain();
            main.Show();
        }

        private async void frmSetParameter_Load(object sender, EventArgs e)
        {
            try
            {
                // TODO - Comment this
                //txtCardCode.Text = "CGB001";

                BankCardConfiguration.BankCardApiDefaultKey = Resources.BankCardApiDefaultKey;
                //var jsonWebSettings = await BankCardLib.Services.ExeWebSettingsService.GetSettings(Resources.Bank);
                //WebSetting = JsonConvert.DeserializeObject<WebSettings>(jsonWebSettings.Settings);

                var jsonWebSettings = await BankCardService.GetBankSettings(Resources.Bank);
                WebSetting = JsonConvert.DeserializeObject<WebSettings>(jsonWebSettings.Settings);
            }
            catch (Exception ex)
            {
                try
                {
                    await BankCardLib.Services.ExeExceptionsService.SaveExeExeption(new BankCardLib.Models.ExeExceptions() { Bank = WebSetting.ProgramName, Message = ex.Message });
                }
                catch { }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
