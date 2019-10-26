using BankCardLib.Extensions;
using BankCardLib.Models;
using BankCardLib.Services;
using CGB.Models;
using CGB.Models.BrowserEvents;
using CGB.Models.Extensions;
using CGB.UAService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CGB
{
    public partial class frmMain : Form
    {
        private ProcessEnum _process;
        private Timer tmrProcess = new Timer();
        private Timer tmrHeartBeat = new Timer();
        private Timer tmrCheckWebError = new Timer();
        private Timer tmrCheckProcess = new Timer();
        private Timer tmrCanRelogin = new Timer();
        private Timer tmrForceCollect = new Timer();
        private Timer tmrCheckMemoryUsage = new Timer();

        private BrowserEvent browserEvent = null;
        private MCUAPP.KML kml = new MCUAPP.KML();
        private decimal QuasiBalance = 0.00M;
        private DateTime LatestTime = DateTime.MinValue;
        private DateTime _lastTime = DateTime.MinValue;
        //decimal balance = 0.00M;
        private bool hasReachLatestDate = false;
        private bool processStop = false;
        private bool hasTriggeredRelogin = false;
        private int loginRetryCount = 0;
        private bool forceCollect = true;
        private bool highMemoryUsage = false;

        private string highMemoryThreshold = System.Configuration.ConfigurationManager.AppSettings["HighMemoryThreshold"];

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        private const int MOUSEEVENTF_HWHEEL = 0x1000;
        private const int MOUSEEVENTF_WHEEL = 0x0800;

        private List<TransactionRecord> CollectionList = new List<TransactionRecord>();
        private List<TransactionRecord> AllCollectedHistory = new List<TransactionRecord>();
        private Dictionary<string, HtmlElement> transactionWithDetails = new Dictionary<string, HtmlElement>();
        List<TempTransRecord> TempBankRecord = new List<TempTransRecord>();

        public ProcessEnum CurrentProcess
        {
            get { return _process; }
            set
            {
                _process = value;
                tlblStatus.Text = _process.ToString();
            }
        }

        public frmMain()
        {
            InitializeComponent();
        }

        private async void frmMain_Load(object sender, EventArgs e)
        {
            try
            {
                if (ModelData.WebSetting.FullAuto && kml.OpenDevice() == 0)
                {
                    MessageBox.Show(ModelData.WebSetting.Messages.NoGhostKey, ModelData.WebSetting.ProgramName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                BrowserEmulation.EnsureBrowserEmulationEnabled(11001);
                BrowserEmulation.SetWebBrowserEmulation(11001);

                browserEvent = new BrowserEvent(wbMain, kml);
                tmrProcess.Interval = 1000;
                tmrProcess.Enabled = false;
                tmrProcess.Tick += tmrProcess_Tick;

                tmrHeartBeat.Interval = 60000;
                tmrHeartBeat.Enabled = true;
                tmrHeartBeat.Tick += TmrHeartBeat_Tick;

                tmrCheckWebError.Interval = 3000;
                tmrCheckWebError.Enabled = true;
                tmrCheckWebError.Tick += tmrCheckWebError_Tick;

                tmrCheckProcess.Interval = 5000;
                tmrCheckProcess.Enabled = true;
                tmrCheckProcess.Tick += tmrCheckProcess_Tick;

                tmrCanRelogin.Interval = 2000;
                tmrCanRelogin.Enabled = false;
                tmrCanRelogin.Tick += tmrCanRelogin_Tick;

                tmrForceCollect.Interval = 600000;
                tmrForceCollect.Enabled = true;
                tmrForceCollect.Tick += tmrForceCollect_Tick;

                tmrCheckMemoryUsage.Interval = 60000;
                tmrCheckMemoryUsage.Enabled = true;
                tmrCheckMemoryUsage.Tick += TmrCheckMemoryUsage_Tick;

                await CGB.UAService.BankCardService.HeartBeat(ModelData.Card.Code);

                wbMain.Navigate(ModelData.WebSetting.Url.MainUrl);
                CurrentProcess = ProcessEnum.Start;
                tmrProcess.Enabled = true;
                //Relogin(ReloginMode.AUTOMATIC);
            }
            catch (Exception ex)
            {
                await ExeExceptionsService.SaveExeExeption(new ExeExceptions() { Bank = ModelData.WebSetting.ProgramName, Message = ex.GetInnerException() });
            }
        }

        private void TmrCheckMemoryUsage_Tick(object sender, EventArgs e)
        {
            Int64 availableMemory = PerformanceInfo.GetPhysicalAvailableMemoryInMiB();
            Int64 totalMemory = PerformanceInfo.GetTotalMemoryInMiB();
            decimal percentFree = ((decimal)availableMemory / (decimal)totalMemory) * 100;
            decimal percentOccupied = 100 - percentFree;
            //File.AppendAllText("memory.txt", percentOccupied.ToString() + "\r\n");
            //highMemoryUsage = percentOccupied >= 80;
            highMemoryUsage = percentOccupied > decimal.Parse(highMemoryThreshold);

            if (highMemoryUsage && 
                (CurrentProcess < ProcessEnum.NavigateAutopayMenu || CurrentProcess > ProcessEnum.NextOrder))
            {
                tmrCheckMemoryUsage.Enabled = false;
                tmrProcess.Enabled = false;
                wbMain.DocumentCompleted -= wbMain_DocumentCompleted;
                tmrCanRelogin.Enabled = true;
            }
        }

        private void tmrForceCollect_Tick(object sender, EventArgs e)
        {
            tmrForceCollect.Enabled = false;

            forceCollect = true;

            tmrForceCollect.Enabled = true;
        }

        private void tmrCheckProcess_Tick(object sender, EventArgs e)
        {
            //try
            //{
            //    tmrCheckProcess.Enabled = false;

            //    if (CurrentProcess == ProcessEnum.SelectDate &&
            //         wbMain.Url.AbsoluteUri.ToString().Contains(ModelData.WebSetting.Url.MainUrl)
            //       )
            //    {
            //        Relogin(ReloginMode.AUTOMATIC);
            //    }

            //    tmrCheckProcess.Enabled = true;
            //}
            //catch { }
        }

        private void tmrCheckWebError_Tick(object sender, EventArgs e)
        {
            try
            {
                tmrCheckWebError.Enabled = false;

                if (CurrentProcess >= ProcessEnum.NavigateAutopayMenu && CurrentProcess <= ProcessEnum.NextOrder &&
                    browserEvent.CheckPaymentError())
                {
                    CurrentProcess = ProcessEnum.NavigateAutopayMenu;
                    tmrProcess.Enabled = true;
                }
                else if (browserEvent.CheckErrorMessage() ||
                         browserEvent.ChecCollectionError() ||
                         browserEvent.CheckLoginError())
                {
                    //hasTriggeredRelogin = true;
                    tmrProcess.Enabled = false;
                    wbMain.DocumentCompleted -= wbMain_DocumentCompleted;
                    //Task.Delay(2000);
                    tmrCanRelogin.Enabled = true;

                    //Relogin(ReloginMode.AUTOMATIC);
                }
            }
            catch { }

            tmrCheckWebError.Enabled = true;
        }

        private void tmrCanRelogin_Tick(object sender, EventArgs e)
        {
            try
            {
                tmrCanRelogin.Enabled = false;

                if (!wbMain.IsBusy)
                {
                    Relogin(ReloginMode.AUTOMATIC);
                    return;
                }

                tmrCanRelogin.Enabled = true;
            }
            catch { }
        }

        private async void TmrHeartBeat_Tick(object sender, EventArgs e)
        {
            try
            {
                await CGB.UAService.BankCardService.HeartBeat(ModelData.Card.Code);
            }
            catch { }
        }

        private void wbMain_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //if (CheckProcessButtonStop()) return;

            if (wbMain.ReadyState == WebBrowserReadyState.Complete)
            {
                tmrProcess.Enabled = true;
            }
        }

        private bool CheckProcessButtonStop()
        {
            if (processStop)
            {
                if (CurrentProcess >= ProcessEnum.PressUKey &&
                   CurrentProcess <= ProcessEnum.NextOrder)
                    return false;
                //if (hasTriggeredRelogin)
                //    Relogin(ReloginMode.AUTOMATIC);

                return true;
            }

            return false;
        }

        private async void tmrProcess_Tick(object sender, EventArgs e)
        {
            if (CheckProcessButtonStop()) return;

            tmrProcess.Enabled = false;

            if (CurrentProcess >= ProcessEnum.Start && CurrentProcess <= ProcessEnum.MainPage)
                await ProcessLogin();
            else if (CurrentProcess >= ProcessEnum.NavigateAutopayMenu && CurrentProcess <= ProcessEnum.NextOrder)
                await ProcessAutopay();
            else if (CurrentProcess >= ProcessEnum.NavigateBalanceMainMenu && CurrentProcess <= ProcessEnum.SaveBalance)
                await ProcessBalance();
            else if (CurrentProcess >= ProcessEnum.NavigateCollectionMainMenu && CurrentProcess <= ProcessEnum.FinishedCollection)
                await ProcessCollection();
        }

        private async Task NextProcess()
        {
            //if (processStop) return;

            if (!ModelData.Transactions.AutopayFinished)
            {
                await BankCardService.GetWithdrawOrdersForExe(ModelData.Card.Id);

                if (ModelData.Orders.CanProcessOrders())
                {
                    //bool processAutopay = true;

                    //if (processAutopay)
                    //{
                    //ModelData.Transactions.AutopayFinished = false;
                    ModelData.Transactions.ProcessedWithdrawal++;
                    CurrentProcess = ProcessEnum.NavigateAutopayMenu;

                    await BankCardService.SetExeLockOrder(ModelData.Orders.FirstOrder.id, true);

                    await ProcessAutopay();
                    //}
                    //else
                    //{
                    //    ModelData.Transactions.AutopayFinished = true;
                    //    await NextProcess();
                    //}
                }
                else
                {
                    ModelData.Transactions.AutopayFinished = true;
                    await NextProcess();
                }
            }
            else if (!ModelData.Transactions.BalanceFinished)
            {
                CurrentProcess = ProcessEnum.NavigateBalanceMainMenu;
                await ProcessBalance();
            }
            else if (!ModelData.Transactions.CollectionFinished)
            {
                CurrentProcess = ProcessEnum.NavigateCollectionMainMenu;
                await ProcessCollection();
            }
            else
            {
                CurrentProcess = ProcessEnum.Done;
                ModelData.Transactions.Clear();
                await NextProcess();
            }
        }

        private async Task ProcessBalance()
        {
            try
            {
                if (CheckProcessButtonStop()) return;

                tmrProcess.Enabled = false;

                switch (CurrentProcess)
                {
                    case ProcessEnum.NavigateBalanceMainMenu:
                        browserEvent.CloseOpenedBalanceTab();
                        browserEvent.CloseOpenedCollectionTab();
                        WebBrowserHelper.ClearCache();

                        if (browserEvent.NavigateMainMenu())
                        {
                            CurrentProcess = ProcessEnum.NavigateMyAccountMenu;
                        }

                        tmrProcess.Enabled = true;
                        break;
                    case ProcessEnum.NavigateMyAccountMenu:
                        if (browserEvent.NavigateMyAccountMenu())
                            CurrentProcess = ProcessEnum.NavigateBalance;

                        tmrProcess.Enabled = true;
                        break;
                    case ProcessEnum.NavigateBalance:
                        if (browserEvent.NavigateBalance())
                        {
                            CurrentProcess = ProcessEnum.GetAccountStatusAndBalance;
                            return;
                        }

                        tmrProcess.Enabled = true;
                        break;
                    case ProcessEnum.GetAccountStatusAndBalance:
                        if (!wbMain.GetData(wbMain.Document.Body).Contains("账户查询"))
                        {
                            tmrProcess.Enabled = true;
                            return;
                        }

                        string cardWebStatus = "";
                        Decimal cardWebBalance = 0.00M;

                        if (browserEvent.NavigateAccountStatusAndBalance(out cardWebStatus, out cardWebBalance))
                        {
                            if (cardWebStatus != "正常")
                            {
                                tmrProcess.Enabled = false;
                                wbMain.DocumentCompleted -= wbMain_DocumentCompleted;
                                await BankCardService.SendStatusNotification(ModelData.Card.Code, cardWebStatus);
                                MessageBox.Show($"{ModelData.Card.AccountName} - {ModelData.Card.AccountNumber} status is {cardWebStatus}");
                                return;
                            }
                            if (QuasiBalance != cardWebBalance || forceCollect)
                            {
                                ModelData.Transactions.BalanceFinished = true;
                                ModelData.Transactions.AutopayFinished = false;
                                forceCollect = false;
                            }
                            else
                            {
                                ModelData.Transactions.BalanceFinished = true;
                                ModelData.Transactions.CollectionFinished = true;
                            }

                            await NextProcess();

                            return;
                        }

                        tmrProcess.Enabled = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                await ExeExceptionsService.SaveExeExeption(new ExeExceptions() { Bank = ModelData.WebSetting.ProgramName, Message = ex.GetInnerException() });
            }
        }
        
        private async Task ProcessAutopay()
        {
            try
            {
                if (CheckProcessButtonStop()) return;

                //tmrProcess.Enabled = false;

                switch (CurrentProcess)
                {
                    case ProcessEnum.NavigateAutopayMenu:
                        browserEvent.CloseOpenedBalanceTab();
                        browserEvent.CloseOpenedCollectionTab();
                        WebBrowserHelper.ClearCache();
                        
                        tmrProcess.Interval = 1000;
                        //btnStop.Enabled = false;
                        //btnContinue.Enabled = false;
                        if(browserEvent.NavigateAutopayMenu())
                            CurrentProcess = ProcessEnum.NavigateAutopaySubMenu;
                        tmrProcess.Enabled = true;
                        break;
                    case ProcessEnum.NavigateAutopaySubMenu:
                        if(browserEvent.NavigateAutopaySubMenu())
                            CurrentProcess = ProcessEnum.TransferPaymentInfo;
                        tmrProcess.Enabled = true;
                        break;
                    case ProcessEnum.TransferPaymentInfo:
                        this.Activate();
                        bool paymentpageNotLoaded = false;
                        if (browserEvent.TransferPaymentInfo(out paymentpageNotLoaded))
                        {
                            if (paymentpageNotLoaded)
                            {
                                CurrentProcess = ProcessEnum.NavigateAutopayMenu;
                                tmrProcess.Enabled = true;
                                return;
                            }
                            CurrentProcess = ProcessEnum.SubmitPaymentInfo;
                        }
                        tmrProcess.Enabled = true;
                        break;
                    case ProcessEnum.SubmitPaymentInfo:
                        //wbMain.DocumentCompleted -= wbMain_DocumentCompleted;
                        if (browserEvent.SubmitPaymentInfo())
                        {
                            //if (CurrentProcess != ProcessEnum.FailedTransfer)
                            //CurrentProcess = ProcessEnum.TransferingPaymentInfo;
                            //CurrentProcess = ProcessEnum.GetPaymentVerification;
                            CurrentProcess = ProcessEnum.CheckPaymentAmount;
                        }
                        tmrProcess.Enabled = true;
                        break;
                    case ProcessEnum.CheckPaymentAmount:
                        bool paymentAmountSame = false;
                        if (browserEvent.CheckPaymentAmount(out paymentAmountSame))
                        {
                            CurrentProcess = paymentAmountSame ? ProcessEnum.GetPaymentVerification : ProcessEnum.NavigateAutopayMenu;
                            //if (paymentAmountSame)
                            //    CurrentProcess = ProcessEnum.GetPaymentVerification;
                            //else
                            //    CurrentProcess = ProcessEnum.NavigateAutopayMenu;
                        }                           

                        tmrProcess.Enabled = true;
                        break;
                    case ProcessEnum.GetPaymentVerification:
                        // TODO: Connect with SubmitPaymentInfo
                        //this.Activate();
                        //wbMain.DocumentCompleted -= wbMain_DocumentCompleted;
                        string captcha = "";
                        if (browserEvent.GetPaymentCaptchaText(out captcha) &&
                           !String.IsNullOrEmpty(captcha))
                        {
                            var elPaymentVerificationCode = wbMain.GetElementByIDEx("checkCode");
                            if (elPaymentVerificationCode != null)
                            {
                                elPaymentVerificationCode.Focus();
                                elPaymentVerificationCode.ClickElement();
                                elPaymentVerificationCode.SetValue(captcha);

                                CurrentProcess = ProcessEnum.TransferingPaymentInfo;
                            }
                        }

                        tmrProcess.Enabled = true;
                        break;
                    case ProcessEnum.TransferingPaymentInfo:
                        if (CheckUKeyError()) return;

                        if (String.IsNullOrEmpty(wbMain.GetElementByIDEx("checkCode").GetAttributeEx()))
                        {
                            CurrentProcess = ProcessEnum.GetPaymentVerification;
                            tmrProcess.Enabled = true;
                        }

                        var elTransferAmount = wbMain.GetElementByIDEx("transferAmt");
                        if (elTransferAmount == null)
                        {
                            tmrProcess.Enabled = true;
                            return;
                        }

                        //MessageBox.Show("Test1");

                        decimal webAmount = 0M;
                        decimal.TryParse(elTransferAmount.GetData(), out webAmount);
                        if (webAmount != ModelData.Orders.FirstOrder.amount)
                        {
                            CurrentProcess = ProcessEnum.NavigateAutopayMenu;
                            tmrProcess.Enabled = true;
                            return;
                        }

                        if (browserEvent.TransferingPaymentInfo())
                        {
                            CurrentProcess = ProcessEnum.EnterUkey;
                        }

                        tmrProcess.Enabled = true;
                        break;
                    ////System.IO.File.AppendAllText("Test.txt", " \r\n " + DateTime.Now.ToString() + " ProcessEnum.TransferingPaymentInfo");
                    ////MessageBox.Show("SubmitPaymentInfo");

                    //// Check Submit Payment Error
                    //string transferPaymentError = "";
                    //if (browserEvent.CheckSubmitPaymentError(out transferPaymentError))
                    //{
                    //    if (!String.IsNullOrEmpty(transferPaymentError) &&
                    //       transferPaymentError.Contains("交易金额不能为空"))
                    //    {
                    //        CurrentProcess = ProcessEnum.NavigateAutopaySubMenu;
                    //        tmrProcess.Enabled = true;
                    //        return;
                    //    }

                    //    tmrProcess.Enabled = false;
                    //    CurrentProcess = ProcessEnum.Done;
                    //    wbMain.DocumentCompleted -= wbMain_DocumentCompleted;
                    //    MessageBox.Show(transferPaymentError, WebSetting.ProgramName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    //    return;
                    //}

                    //if (browserEvent.CheckTransferSummaryPage())
                    //    ModelData.Orders.FirstOrder.fee = browserEvent.GetServiceFee();

                    //if (browserEvent.CheckSafetyTool())
                    //{
                    //    if (wrongDongleKey)
                    //    {
                    //        wrongDongleKey = false;
                    //        return;
                    //    }
                    //}

                    //if (browserEvent.isCorrectAmount())
                    //{
                    //    CurrentProcess = ProcessEnum.EnterATMPassword;
                    //}
                    //else
                    //{
                    //    CurrentProcess = ProcessEnum.NavigateAutopayMenu;
                    //}
                    ////await ProcessAutopay();
                    //tmrProcess.Enabled = true;
                    //break;
                    //case ProcessEnum.EnterATMPassword:
                    //    this.Activate();

                    //    //System.IO.File.AppendAllText("Test.txt", " \r\n " + DateTime.Now.ToString() + " ProcessEnum.EnterATMPassword");
                    //    wrongDongleKey = false;
                    //    if (WebSetting.FullAuto)
                    //    {
                    //        //await Wait(2);
                    //        //ghostkey focus
                    //        for (int i = 0; i < ModelData.Card.PayPassword.Length; i++)
                    //        {
                    //            kml.KeyPress(ModelData.Card.PayPassword[i].ToString(), 1);
                    //        }
                    //        //await Wait(2);
                    //        //click submit button
                    //        //wbMain.GetElementByTag("input", "value", "提交").InvokeMember("click");
                    //        tmrProcess.Interval = 2000;
                    //        CurrentProcess = ProcessEnum.SubmitATMPassword;
                    //        tmrProcess.Enabled = true;
                    //    }
                    //    else
                    //    {
                    //        CurrentProcess = ProcessEnum.CheckTransferring;
                    //        wbMain.DocumentCompleted -= wbMain_DocumentCompleted;
                    //        wbMain.DocumentCompleted += wbMain_DocumentCompleted;
                    //        tmrProcess.Enabled = true;
                    //        //await ProcessAutopay();
                    //    }
                    //    break;
                    //case ProcessEnum.SubmitATMPassword:

                    //    if (browserEvent.SubmitATMPassword())

                    //        await BankCardService.ShowUkey(ModelData.Orders.FirstOrder.id);
                    //    CurrentProcess = ProcessEnum.EnterUkey;

                    //    tmrProcess.Interval = 1000;
                    //    tmrProcess.Enabled = true;
                    //    break;
                    case ProcessEnum.EnterUkey:
                        //wbMain.DocumentCompleted -= wbMain_DocumentCompleted;
                        //wbMain.DocumentCompleted += wbMain_DocumentCompleted;

                        if (CheckUKeyError()) return;

                        //// TODO: Removed
                        //wbMain.DocumentCompleted -= wbMain_DocumentCompleted;
                        //tmrProcess.Enabled = false;
                        //CurrentProcess = ProcessEnum.PressUKey;
                        //return;

                        //System.IO.File.AppendAllText("Test.txt", " \r\n " + DateTime.Now.ToString() + " ProcessEnum.EnterUkey");

                        //if (WebSetting.FullAuto)
                        //{
                        if (browserEvent.ContainsUKeyDialog("USB-Key Pin", "验证U宝口令", "Key盾密码"))
                        {
                            await BankCardService.ShowUkey(ModelData.Orders.FirstOrder.id);

                            this.Activate();

                            ////uKey.InputText(WindowsInput.Native.VirtualKeyCode.TAB);                            
                            ////kml.Delay(1000);
                            //for (int i = 0; i < ModelData.Card.UkeyPassword.Length; i++)
                            //{
                            //    kml.KeyPress(ModelData.Card.UkeyPassword[i].ToString(), 1);
                            //    await Task.Delay(100);
                            //}
                            ////await Utility.Wait(1);
                            //await Task.Delay(1000);

                            //SendKeys.Send("{TAB}");

                            Rectangle screenRect = Screen.PrimaryScreen.WorkingArea;
                            var size = new Size((int)(screenRect.Width / 2), (int)(screenRect.Height / 2)); // set the size of the form
                            var center = new Point(screenRect.Width / 2 - ClientSize.Width / 2, screenRect.Height / 2 - ClientSize.Height / 2); // Center the Location of

                            //Cursor.Position = new Point(size.Width - 65, size.Height + 45);
                            var position = new Point(size.Width - 65, size.Height + 45);
                            //mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                            //await Task.Delay(100);
                            //mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);

                            File.WriteAllText("PressUKeyInfo", "false");
                            System.Diagnostics.Process.Start("CGBAutoInputUKey.exe", string.Join("|", ModelData.Card.UkeyPassword, position.X, position.Y));



                            //SendKeys.Send("{ENTER}");
                            //kml.CombinationKeyPress("Tab", "Enter", "", "", "", "", 1);
                            //kml.CombinationKeyPress("Space", "Space", "Space", "Space", "Space", "Enter", 1);
                            //CurrentProcess = ProcessEnum.CheckTransferring;
                            CurrentProcess = ProcessEnum.PressUKey;
                            tmrProcess.Interval = 1000;
                            //tmrProcess.Enabled = true;
                        }

                            //if (browserEvent.HasTransferResult())
                            //{
                            //    CurrentProcess = ProcessEnum.Transferring;
                            //}
                        //}
                        //else
                        //{
                        //    CurrentProcess = ProcessEnum.CheckTransferring;
                        //    wbMain.DocumentCompleted -= wbMain_DocumentCompleted;
                        //    //tmrProcess.Enabled = true;
                        //    //await ProcessAutopay();
                        //}

                        //// Check Submit Payment Error
                        //string submitPaymentError = "";
                        //if (browserEvent.CheckSubmitPaymentError(out submitPaymentError))
                        //{
                        //    tmrProcess.Enabled = false;
                        //    CurrentProcess = ProcessEnum.Done;
                        //    wbMain.DocumentCompleted -= wbMain_DocumentCompleted;
                        //    MessageBox.Show(submitPaymentError, WebSetting.ProgramName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        //}

                        tmrProcess.Enabled = true;

                        break;
                    case ProcessEnum.PressUKey:
                        tmrProcess.Interval = 1000;

                        if (CheckUKeyError()) return;

                        //if (browserEvent.ContainsUKeyDialog("USB-Key Pin", "验证U宝口令"))
                        //{
                        //    CurrentProcess = ProcessEnum.EnterUkey;
                        //    tmrProcess.Enabled = true;
                        //    return;
                        //}

                        if (browserEvent.HasTransferResult())
                        {
                            CurrentProcess = ProcessEnum.CheckTransferring;
                            return;
                        }

                        var pressUKeyInfo = Convert.ToBoolean(File.ReadAllText("PressUKeyInfo"));
                        if (pressUKeyInfo)
                        {
                            try
                            {
                                var pressUKeyResult = await BankCardService.PressUKey();
                            }
                            catch { }
                            
                            File.WriteAllText("PressUKeyInfo", "false");
                            CurrentProcess = ProcessEnum.CheckTransferring;
                        }

                        tmrProcess.Enabled = true;
                        break;
                    case ProcessEnum.CheckTransferring:
                        if (CheckUKeyError()) return;

                        if (browserEvent.CheckPaymentError())
                        {
                            CurrentProcess = ProcessEnum.NavigateAutopayMenu;
                            tmrProcess.Enabled = true;
                            return;
                        }

                        if (browserEvent.HasTransferResult())
                            CurrentProcess = ProcessEnum.GetTransferResult;

                        tmrProcess.Enabled = true;
                        break;
                    //case ProcessEnum.Transferring:
                    //    //_exeHubClient.DoneRequestUKeyInput();

                    //    CurrentProcess = ProcessEnum.GetTransferResult;
                    //    await ProcessAutopay();
                    //    break;
                    ////case ProcessEnum.AtmPasswordError:
                    ////    break;
                    case ProcessEnum.GetTransferResult:
                        //System.Diagnostics.Debugger.Launch();
                        //if (CheckUKeyError()) return;
                        //tmrProcess.Enabled = true;
                        //return;

                        //chargeFee

                        // TODO: Check for error messages
                        //WebErrors webErr = null;
                        //if (!String.IsNullOrEmpty(browserEvent.CheckWebErrors(out webErr)))
                        //    return;

                        string notes = "";
                        bool result = browserEvent.GetTransferredResult(out notes);
                        if (result)
                        {
                            var orderStatus = OrderStatus.AUTOCLOSE;

                            // TODO: Add Payment Error Messages
                            var invalidNotes = new String[] { "EBIBP011",
                                                              "账号、户名不符" };

                            if (!String.IsNullOrEmpty(notes) &&
                                invalidNotes.Any(x => notes.Contains(x)))
                            {
                                orderStatus = OrderStatus.INVALID;
                            }

                            //ModelData.Orders.SetOrderStatus(OrderStatus.AUTOCLOSE, notes);
                            ModelData.Orders.SetOrderStatus(orderStatus, notes);
                            await Task.Delay(3000);
                            if (orderStatus == OrderStatus.AUTOCLOSE)
                            {
                                setTempBankRecord();
                                TempTransRecord temp = new TempTransRecord
                                {
                                    order_id = ModelData.Orders.FirstOrder.id,
                                    account_name = ModelData.Orders.FirstOrder.account_name,
                                    account_number = ModelData.Orders.FirstOrder.account_number,
                                    amount = ModelData.Orders.FirstOrder.amount,
                                    date_hour_mins = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
                                };
                                TempBankRecord.Add(temp);
                                saveTempOrderDataToFile(TempBankRecord);
                            }
                            CurrentProcess = ProcessEnum.Transferred;
                            await ProcessAutopay();
                        }
                        else
                            tmrProcess.Enabled = true;

                        break;
                    case ProcessEnum.Transferred:
                    case ProcessEnum.FailedTransfer:
                        //Transactions.CollectionFinished = false;
                        //pressedCtrlShiftH = false;

                        if (!String.IsNullOrEmpty(ModelData.Orders.FirstOrder.notes))
                        {
                            //if (!String.IsNullOrEmpty(Orders.FirstOrder.Fee.ToString()))
                            //    Orders.FirstOrder.Fee = Orders.FirstOrder.Fee.ToString().Replace("（，）", "");

                            //await service.CloseOrder(OrderList.FirstOrder, CurrentProcess.ToString());


                            await BankCardService.WithdrawEdit(ModelData.Orders.FirstOrder.id, (int)ModelData.Orders.FirstOrder.status, ModelData.Orders.FirstOrder.notes, (DateTime)ModelData.Orders.FirstOrder.operating_time);

                            //isPopUpRelogin = false;
                            ModelData.Orders.Withdrawals = new List<UAService.WithdrawOrder>();

                            //btnStop.Enabled = true;
                            //btnContinue.Enabled = false;

                            if (!highMemoryUsage)
                            {
                                await NextProcess();
                            }
                            //CurrentProcess = ProcessEnum.NextOrder;
                            //tmrProcess.Enabled = true;
                        }
                        else
                            MessageBox.Show(ModelData.WebSetting.Messages.NoteIsEmpty, ModelData.WebSetting.ProgramName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                        if (highMemoryUsage)
                        {
                            CurrentProcess = ProcessEnum.Start;
                            tmrCheckMemoryUsage.Enabled = false;
                            tmrProcess.Enabled = false;
                            wbMain.DocumentCompleted -= wbMain_DocumentCompleted;
                            tmrCanRelogin.Enabled = true;
                            return;
                        }

                        break;
                        //    //case ProcessEnum.NextOrder:
                        //    //    // Check


                        //    //    CurrentProcess = ProcessEnum.NavigateMyAccountMenu;
                        //    //    tmrProcess.Enabled = true;
                        //    //    break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                await ExeExceptionsService.SaveExeExeption(new ExeExceptions() { Bank = ModelData.WebSetting.ProgramName, Message = ex.GetInnerException() });
            }
        }

        private Boolean CheckUKeyError()
        {
            if (!String.IsNullOrEmpty(browserEvent.CheckUkeyError()))
            {
                wbMain.GetElementByIDEx("_verifyImage").ClickElement();
                wbMain.GetElementByIDEx("checkCode").SetValue("");
                CurrentProcess = ProcessEnum.GetPaymentVerification;
                //CurrentProcess = ProcessEnum.SubmitPaymentInfo;
                tmrProcess.Enabled = true;
                return true;
            }

            return false;
        }

        private async Task ProcessCollection()
        {
            try
            {
                if (CheckProcessButtonStop()) return;

                tmrProcess.Enabled = false;

                switch (CurrentProcess)
                {
                    case ProcessEnum.NavigateCollectionMainMenu:
                        browserEvent.CloseOpenedBalanceTab();
                        browserEvent.CloseOpenedCollectionTab();

                        if (browserEvent.NavigateMainMenu())
                        {
                            CurrentProcess = ProcessEnum.NavigateMyAccountCollectionMenu;
                        }

                        tmrProcess.Enabled = true;
                        break;
                    case ProcessEnum.NavigateMyAccountCollectionMenu:
                        if (browserEvent.NavigateMyAccountMenu())
                            CurrentProcess = ProcessEnum.NavigateCollection;

                        tmrProcess.Enabled = true;
                        break;
                    case ProcessEnum.NavigateCollection:
                        if (browserEvent.NavigateCollection())
                            CurrentProcess = ProcessEnum.SelectDate;

                        tmrProcess.Enabled = true;
                        break;
                    case ProcessEnum.SelectDate:
                        LatestTime = await CGB.UAService.BankCardService.GetLatestDate(ModelData.Card.Code);

                        // TODO: Remove
                        //LatestTime = DateTime.Parse("2019-04-01");

                        hasReachLatestDate = false;
                        if (browserEvent.SelectDate(LatestTime))
                            CurrentProcess = ProcessEnum.GetCollectionList;

                        tmrProcess.Enabled = true;
                        break;
                    case ProcessEnum.GetCollectionList:
                        bool collectionAccountResult = false;
                        if (browserEvent.CheckCollectionAccount(out collectionAccountResult))
                        {
                            if (!collectionAccountResult)
                            {
                                tmrProcess.Enabled = false;
                                wbMain.DocumentCompleted -= wbMain_DocumentCompleted;
                                MessageBox.Show("The bankcard account is not same with the website account.\r\nCGB Exe will close.", "CGB");
                                Application.Exit();
                                return;
                            }
                        }
                        else
                        {
                            tmrProcess.Enabled = true;
                            return;
                        }

                        if (browserEvent.GetTotalRows(out int total))
                        {
                            List<TransactionRecord> historyList = new List<TransactionRecord>();
                            transactionWithDetails = new Dictionary<string, HtmlElement>();
                            if (browserEvent.GetCollectionList(out historyList, LatestTime, out hasReachLatestDate, out transactionWithDetails))
                            {
                                CollectionList.AddRange(historyList.Where(x => x != null));
                                //CurrentProcess = ProcessEnum.NextPage;
                                CurrentProcess = ProcessEnum.NavigateCollectionDetails;
                            }

                            if (!hasReachLatestDate && CollectionList.Count == total)
                            {
                                hasReachLatestDate = true;
                            }
                        }

                        tmrProcess.Enabled = true;
                        break;
                    case ProcessEnum.NavigateCollectionDetails:
                        if (transactionWithDetails.Count > 0)
                        {
                            foreach (var item in transactionWithDetails)
                            {
                                item.Value.ClickElement();
                                item.Value.Children[0].ClickElement();
                                CurrentProcess = ProcessEnum.GetCollectionDetails;
                                //tmrProcess.Enabled = true;
                                break;
                            }
                        }
                        else
                        {
                            CurrentProcess = ProcessEnum.NextPage;
                        }
                        
                        tmrProcess.Enabled = true;
                        break;
                    case ProcessEnum.GetCollectionDetails:
                        if (transactionWithDetails.Count > 0)
                        {
                            var details = transactionWithDetails.First();
                            var data = CollectionList.Where(x => x.detail == details.Key).FirstOrDefault();
                            if (data != null)
                            {
                                //browserEvent.NavigateCollectionDetails(data, );
                                if(!browserEvent.GetCollectionDetails(data))
                                {
                                    tmrProcess.Enabled = true;
                                    return;
                                }
                            }
                            else
                            {
                                CurrentProcess = ProcessEnum.NavigateCollectionDetails;
                            }

                            transactionWithDetails.Remove(transactionWithDetails.First().Key);
                            CurrentProcess = ProcessEnum.NavigateCollectionDetails;
                        }
                        else
                        {
                            CurrentProcess = ProcessEnum.NextPage;
                        }

                        tmrProcess.Enabled = true;
                        break;
                    case ProcessEnum.NextPage:

                        if (!hasReachLatestDate && browserEvent.NextPage())
                        {
                            CurrentProcess = ProcessEnum.GetCollectionList;
                        }
                        else
                        {
                            CurrentProcess = ProcessEnum.SaveCollection;

                            // Get Last Balance
                            var lastTransaction = CollectionList.OrderBy(x => x.transaction_time).LastOrDefault();
                            if (lastTransaction != null)
                            {
                                QuasiBalance = lastTransaction.balance;
                                _lastTime = DateTime.Parse(lastTransaction.transaction_time);
                            }

                            var count = CollectionList.Where(x => DateTime.Parse(x.transaction_time) > LatestTime).ToList().Count;
                            if (count > 0)
                                CurrentProcess = ProcessEnum.SaveCollection;
                            else
                            {
                                CurrentProcess = ProcessEnum.FinishedCollection;
                                LatestTime = _lastTime;
                            }
                        }

                        tmrProcess.Enabled = true;
                        break;

                    case ProcessEnum.SaveCollection:
                        if (CollectionList.Count > 0)
                        {
                            setTempBankRecord();
                            var filterData = CollectionList.Where(x => DateTime.Parse(x.transaction_time) > LatestTime &&
                                                                       !AllCollectedHistory.Any(y => y.detail == x.detail)).ToList();

                            if (filterData.Count > 0)
                            {
                                await CGB.UAService.BankCardService.CollectionHistory(ModelData.Card.Code, QuasiBalance, filterData.ToList(), TempBankRecord);
                                AllCollectedHistory.AddRange(filterData);

                                TempBankRecord = new List<TempTransRecord>();
                                saveTempOrderDataToFile(TempBankRecord);
                            }
                            LatestTime = _lastTime;
                        }

                        CollectionList.Clear();
                        CurrentProcess = ProcessEnum.FinishedCollection;
                        tmrProcess.Enabled = true;

                        break;
                    case ProcessEnum.FinishedCollection:
                        ModelData.Transactions.CollectionFinished = true;
                        await NextProcess();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                await ExeExceptionsService.SaveExeExeption(new ExeExceptions() { Bank = ModelData.WebSetting.ProgramName, Message = ex.GetInnerException() });
            }

        }

        private async Task ProcessLogin()
        {
            try
            {
                //if (processStop) return;
                if (CheckProcessButtonStop()) return;

                //tmrProcess.Enabled = false;

                switch (CurrentProcess)
                {
                    case ProcessEnum.Start:
                        if (loginRetryCount == 2)
                        {
                            Relogin(ReloginMode.AUTOMATIC);
                            return;
                        }

                        loginRetryCount++;
                        await Task.Delay(2000);
                        this.wbMain.Focus();
                        CurrentProcess = ProcessEnum.Login;
                        tmrProcess.Enabled = true;
                        break;

                    case ProcessEnum.Login:
                        this.Activate();

                        if (browserEvent.LoginWebsite(this))
                            CurrentProcess = ProcessEnum.EnterLoginPassword;

                        tmrProcess.Enabled = true;
                        break;

                    case ProcessEnum.EnterLoginPassword:
                        this.Activate();
                        if(!await browserEvent.EnterLoginPassword(this))
                        {
                            tmrProcess.Enabled = true;
                            return;
                        }
                        CurrentProcess = ProcessEnum.EnterCaptcha;
                        tmrProcess.Interval = 2000;
                        tmrProcess.Enabled = true;
                        break;

                    case ProcessEnum.EnterCaptcha:
                        tmrProcess.Interval = 1000;
                        this.Activate();
                        string captcha = "";
                        if (browserEvent.GetLoginCaptchaText(out captcha) &&
                           !String.IsNullOrEmpty(captcha))
                        {
                            SendKeys.Send(captcha);
                            //SendKeys.Send("t7tu");
                            CurrentProcess = ProcessEnum.SubmitLogin;
                        }

                        tmrProcess.Enabled = true;
                        break;
                    case ProcessEnum.SubmitLogin:
                        if (browserEvent.SubmitLogin())
                            CurrentProcess = ProcessEnum.MainPage;

                        tmrProcess.Enabled = true;
                        break;

                    case ProcessEnum.MainPage:
                        ModelData.Transactions.Clear();

                        // Check Wrong Password
                        string webError = "";
                        if (browserEvent.CheckWrongPassword(out webError))
                        {
                            // US003017 - 异常描述：加密方式上送错误
                            var errorList = new string[] { "US003017" };
                            if(errorList.Any(x => webError.Contains(x)))
                            {
                                tmrProcess.Enabled = false;
                                wbMain.DocumentCompleted -= wbMain_DocumentCompleted;
                                tmrCanRelogin.Enabled = true;
                            }
                            else if(webError.Contains("PA020128")) // 异常描述：登录名或密码错误，请输入正确的登录名和登录密码。
                            {
                                btnStop.PerformClick();
                                tmrProcess.Enabled = false;
                                tmrCheckProcess.Enabled = false;
                                //tmrCheckWebError.Enabled = false;
                                wbMain.DocumentCompleted -= wbMain_DocumentCompleted;

                                MessageBox.Show(webError);
                            }
                            //else
                            //{
                            //    tmrProcess.Enabled = false;
                            //    wbMain.DocumentCompleted -= wbMain_DocumentCompleted;
                            //    tmrCanRelogin.Enabled = true;
                            //}
                            
                            return;
                        }

                        if (wbMain.GetData(wbMain.Document.Body).Contains("首页"))
                        {
                            loginRetryCount = 0;
                            //CurrentProcess = ProcessEnum.NavigateMyAccountMenu;
                            ModelData.CardLoginHistory.Id = 0;
                            ModelData.CardLoginHistory.CardId = ModelData.Card.Id;
                            ModelData.CardLoginHistory.IsLogin = true;
                            await BankCardService.SaveCardHistory(ModelData.CardLoginHistory);
                            await NextProcess();
                        }

                        tmrProcess.Enabled = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                await ExeExceptionsService.SaveExeExeption(new ExeExceptions() { Bank = ModelData.WebSetting.ProgramName, Message = ex.GetInnerException() });
            }
        }

        private void tsRelogin_Click(object sender, EventArgs e)
        {
            Relogin(ReloginMode.MANUAL);
        }

        private void Relogin(ReloginMode mode)
        {
            var result = DialogResult.OK;
            if (mode == ReloginMode.MANUAL)
                result = MessageBox.Show(ModelData.WebSetting.Messages.Relogin, ModelData.WebSetting.ProgramName, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                tmrCheckMemoryUsage.Enabled = false;
                highMemoryUsage = false;
                tmrProcess.Enabled = false;
                hasTriggeredRelogin = false;
                tmrCheckWebError.Enabled = false;
                wbMain.DocumentCompleted -= wbMain_DocumentCompleted;
                CurrentProcess = ProcessEnum.Start;
                QuasiBalance = 0;
                loginRetryCount = 0;
                //ModelData.CardLoginHistory.IsLogin = false;
                //await BankCardService.SaveCardHistory(ModelData.CardLoginHistory);
                wbMain.Navigate("about:blank");
                Task.Delay(2000);
                wbMain.Navigate(ModelData.WebSetting.Url.MainUrl);
                wbMain.DocumentCompleted += wbMain_DocumentCompleted;
                this.Activate();
                wbMain.Focus();
                btnContinue.PerformClick();
                tmrProcess.Enabled = true;
                tmrCheckWebError.Enabled = true;
                tmrCheckMemoryUsage.Enabled = true;
            }
        }

        private async void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            ModelData.CardLoginHistory.IsLogin = false;
            await BankCardService.SaveCardHistory(ModelData.CardLoginHistory);
            if(ModelData.Orders.FirstOrder != null)
                await BankCardService.SetExeLockOrder(ModelData.Orders.FirstOrder.id, false);
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void btnContinue_Click(object sender, EventArgs e)
        {
            btnStop.Enabled = true;
            btnContinue.Enabled = false;
            processStop = false;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            btnStop.Enabled = false;
            btnContinue.Enabled = true;
            processStop = true;
        }

        private void wbMain_NewWindow(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
        }

        private string getTempOrderDataFile()
        {
            try
            {
                var temp_payment_order = System.IO.File.ReadAllText("TempPaymentOrder.txt", System.Text.Encoding.UTF8);

                if (!String.IsNullOrEmpty(temp_payment_order))
                {
                    return temp_payment_order;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception e)
            { 
                return "";
            }
        }

        private void saveTempOrderDataToFile(List<TempTransRecord> temp_order)
        {
            if (temp_order.Count > 0)
            {
                var temp_str_order = JsonConvert.SerializeObject(temp_order);
                System.IO.File.WriteAllText("TempPaymentOrder.txt", temp_str_order);
            }
            else
            {
                System.IO.File.WriteAllText("TempPaymentOrder.txt", "");
            }
        }

        private void setTempBankRecord()
        {
            var temp_string_bank_record = getTempOrderDataFile();
            if (!string.IsNullOrEmpty(temp_string_bank_record))
            {
                TempBankRecord = JsonConvert.DeserializeObject<List<TempTransRecord>>(temp_string_bank_record);
            }
        }
    }
}
