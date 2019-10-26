using AForge.Imaging.Filters;
using BankCardLib.Extensions;
using CGB.Models.Extensions;
using CGB.UAService;
using mshtml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using tessnet2;
using WindowsInput;
using WindowsInput.Native;
using static CGB.OCRLib.OCR;

namespace CGB.Models.BrowserEvents
{
    public class BrowserEvent
    {
        private WebBrowser _wbMain = null;
        private MCUAPP.KML _kml = null;
        InputPaymentAction inputPaymentAction = InputPaymentAction.account_name;
        InputSimulator simulator = new InputSimulator();
        public BrowserEvent(WebBrowser wbMain, MCUAPP.KML kml)
        {
            _wbMain = wbMain;
            _kml = kml;
        }

        #region FindWindow

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        private bool ContainsUKeyDialog()
        {
            var titles = new string[] { "Message from webpage", "来自网页的消息" };
            var buttons = new string[] { "OK", "确定", "确定","好的" };
            foreach (var title in titles)
            {
                try
                {
                    foreach (var button in buttons)
                    {
                        IntPtr hwnd = FindWindow("#32770", title);
                        hwnd = FindWindowEx(hwnd, IntPtr.Zero, "Button", button);

                        if (hwnd != IntPtr.Zero)
                        {
                            uint message = 0xf5;
                            SendMessage(hwnd, message, IntPtr.Zero, IntPtr.Zero);
                            return true;
                        }
                    }
                }
                catch { }       
            }

            return false;
        }

        #endregion


        #region Login

        internal bool LoginWebsite(Form parentForm)
        {
            var elLogin = _wbMain.GetElementByID("loginId");

            if (elLogin != null)
            {
                Point parentPoint = new Point(parentForm.Left, parentForm.Top);
                int xPos = parentPoint.X + getXoffset(elLogin) + 20;
                int yPos = parentPoint.Y + getYoffset(elLogin) + 10;

                var p = new Point(xPos, yPos);
                Cursor.Position = p;
                simulator.Mouse.LeftButtonClick();

                elLogin.Focus();
                elLogin.SetAttribute("value", ModelData.Card.AccountNumber);
                //SendKeys.Send("{TAB}");

                return true;
            }

            return false;
        }

        internal async Task<Boolean> EnterLoginPassword(Form parentForm)
        {
            var elLogin = _wbMain.GetElementByID("loginId");
            if (elLogin != null)
            {
                //elLogin.Focus();
                //SendKeys.Send("{TAB}");
                //await Task.Delay(2000);

                //int xPosParent = getXoffset(elLogin);
                //int yPosParent = getYoffset(elLogin);

                Point parentPoint = new Point(parentForm.Left, parentForm.Top);
                int xPos = parentPoint.X + getXoffset(elLogin) + 20;
                int yPos = parentPoint.Y + getYoffset(elLogin) + 10 + 82 + 85;

                var p = new Point(xPos, yPos);
                Cursor.Position = p;
                simulator.Mouse.LeftButtonClick();
                //simulator.Mouse.LeftButtonClick();

                //await Task.Delay(2000);

                _kml.CombinationKeyPress("Backspace", "", "", "", "", "", 1);

                await Task.Delay(100);
                for (int i = 0; i < ModelData.Card.LoginPassword.Length; i++)
                {
                    _kml.KeyPress(ModelData.Card.LoginPassword[i].ToString(), 1);
                    await Task.Delay(400);
                }

                return true;
            }

            return false;

            //simulator.Keyboard.KeyPress(VirtualKeyCode.VK_A);
            //await Task.Delay(500);
            //simulator.Keyboard.KeyPress(VirtualKeyCode.VK_A);
            //await Task.Delay(500);
            //simulator.Keyboard.KeyPress(VirtualKeyCode.VK_1);
            //await Task.Delay(500);
            //simulator.Keyboard.KeyPress(VirtualKeyCode.VK_2);
            //await Task.Delay(500);
            //simulator.Keyboard.KeyPress(VirtualKeyCode.VK_3);
            //await Task.Delay(500);
            //simulator.Keyboard.KeyPress(VirtualKeyCode.VK_3);
            //await Task.Delay(500);
            //simulator.Keyboard.KeyPress(VirtualKeyCode.VK_2);
            //await Task.Delay(500);
            //simulator.Keyboard.KeyPress(VirtualKeyCode.VK_1);
            //await Task.Delay(500);
        }

        internal bool GetLoginCaptchaText(out string captchaText)
        {
            captchaText = "";

            try
            {
                // Focus in Captcha HTMLElement
                var elLogin = _wbMain.GetElementByID("loginId");
                if (elLogin != null)
                {
                    elLogin.Focus();
                    SendKeys.Send("{TAB}");
                    SendKeys.Send("{TAB}");
                }

                var captchaImageTry = 0;

                // Get Image From Browser
                Bitmap img = null;
                do
                {
                    img = GetLoginCaptchaImage();
                    captchaImageTry++;

                    if (captchaImageTry >= 5)
                    {
                        RenewCaptcha();
                        return false;
                    }
                }
                while (img.IsNull());

                if (!img.IsNull())
                {
                    var captcha = SolveCaptcha(img);

                    if (!String.IsNullOrEmpty(captcha))
                        captcha = captcha.Replace(" ", "").Trim();

                    if (String.IsNullOrEmpty(captcha) ||
                        (!String.IsNullOrEmpty(captcha) && (captcha.Length < 4 || captcha.Length > 4)))
                    {
                        // Nothing
                    }
                    else
                    {
                        captchaText = captcha;
                        return true;
                    }
                }
            }
            catch (Exception ex) { }

            RenewCaptcha();

            return false;
        }

        private void RenewCaptcha()
        {
            var elChangeCaptchaClick = _wbMain.GetElementByID("verifyImg");
            if (elChangeCaptchaClick != null)
            {
                elChangeCaptchaClick.ClickElement();
            }
        }

        internal bool CheckErrorMessage()
        {
            return ContainsUKeyDialog();
        }

        private Bitmap GetLoginCaptchaImage()
        {
            Bitmap image = null;
            IHTMLDocument2 doc = (IHTMLDocument2)_wbMain.Document.DomDocument;
            HtmlDocument d = _wbMain.Document;
            IHTMLControlRange imgRange1 = (IHTMLControlRange)((HTMLBody)doc.body).createControlRange();

            foreach (IHTMLImgElement img in doc.images)
            {
                imgRange1.add((IHTMLControlElement)img);
                object a = Clipboard.GetDataObject();

                imgRange1.execCommand("Copy", false, null);
                image = (Bitmap)Clipboard.GetDataObject().GetData(DataFormats.Bitmap);
            }

            return image;
        }

        internal bool CheckLoginError()
        {
            var loginError = new String[] { "，此为必输项", "请输入验证码，此为必输项" };
            return loginError.Any(x => _wbMain.Document.Body.GetData().Contains(x));
        }

        private string SolveCaptcha(Image img)
        {
            Bitmap imagem = new Bitmap(img);
            SetPixelColor(imagem, false);
            SetPixelColor(imagem, false);
            string filename = "test/" + Guid.NewGuid().ToString() + ".png";

            SetPixelColor(imagem, false);
            imagem = ProcessImage(imagem);
            imagem.Save(filename);

            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(filename);
            var imageClone = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            Erosion erosion = new Erosion();
            Dilatation dilatation = new Dilatation();
            Invert inverter = new Invert();
            ColorFiltering cor = new ColorFiltering();

            cor.Red = new AForge.IntRange(150, 255);
            cor.Green = new AForge.IntRange(150, 255);
            cor.Blue = new AForge.IntRange(128, 255);
            Opening open = new Opening();
            BlobsFiltering bc = new BlobsFiltering();
            Closing close = new Closing();
            GaussianSharpen gs = new GaussianSharpen();
            ContrastCorrection cc = new ContrastCorrection();
            FiltersSequence seq = new FiltersSequence(cc, gs, inverter, cor, bc, cor, cc, cor, bc);

            var image = seq.Apply(imageClone);
            var byteImg = ImageToByte(image);
            var result = OCR(ByteToImage(byteImg));

            return result;
        }

        private static void SetPixelColor(Bitmap imgBmp, bool hasBeenCleared = true)
        {
            var bgColor = Color.White;
            var textColor = Color.Black;
            for (var x = 0; x < imgBmp.Width; x++)
            {
                for (var y = 0; y < imgBmp.Height; y++)
                {
                    var pixel = imgBmp.GetPixel(x, y);
                    var isCloserToWhite = hasBeenCleared ? ((pixel.R + pixel.G + pixel.B) / 3) > 180 : ((pixel.R + pixel.G + pixel.B) / 3) > 120;
                    imgBmp.SetPixel(x, y, isCloserToWhite ? bgColor : textColor);
                }
            }
        }

        private static Bitmap ProcessImage(Bitmap image, byte blueThreshold = 139)
        {
            try
            {
                var imageClone = image.Clone(new Rectangle(0, 0, image.Width, image.Height), image.PixelFormat);

                var info = new CaptchaInfo();
                imageClone = imageClone.AdjustCurves2()
                             .GetCaptchaInfo(ref info);

                imageClone = image.AdjustCurves2()
                             .CleanUnecessaryPixel(info)
                             .BlackenColor()
                             .CleanUnecessaryPixel(info)
                             .Sharpen();


                return imageClone;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        private byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        private Bitmap ByteToImage(byte[] byteArray)
        {
            ImageConverter ic = new ImageConverter();
            Image img = (Image)ic.ConvertFrom(byteArray);
            Bitmap bitmap = new Bitmap(img);
            return bitmap;
        }

        public string OCR(System.Drawing.Bitmap image)
        {
            string text = "";
            string text2 = "";
            try
            {
                string tessdata = @"tessdata";
                Tesseract tesseract = new Tesseract();
                tesseract.Clear();
                tesseract.SetVariable("tessedit_char_whitelist", "123456789abcdefghijklmnpqrstuvwxyz");
                tesseract.SetVariable("tessedit_adaption_debug", true);
                tesseract.SetVariable("tessedit_ambigs_training ", true);
                tesseract.Init(tessdata, "eng", false);

                Bitmap originalImage = image;
                double needdedHeigth = Convert.ToDouble(image.Height + 10);
                double faktor = needdedHeigth / (double)(originalImage.Height);
                int newWidth = Convert.ToInt32(faktor * (double)originalImage.Width);
                Bitmap ORCImage = new Bitmap(originalImage, newWidth, Convert.ToInt32(needdedHeigth));

                System.Collections.Generic.List<Word> list = tesseract.DoOCR(ORCImage, System.Drawing.Rectangle.Empty);
                System.Collections.Generic.List<Word> list2 = tesseract.DoOCR(image, System.Drawing.Rectangle.Empty);
                image.Dispose();

                foreach (Word current in list)
                {
                    text2 += current.Text;
                }

                foreach (Word current3 in list2)
                {
                    text += current3.Text;
                }

                if (text.Length != 4)
                {
                    text = text2;
                }
            }
            catch (System.Exception value)
            {
                System.Console.WriteLine(value);
            }
            return text.Replace(" ", "");
        }

        internal bool SubmitLogin()
        {
            var elSubmit = _wbMain.GetElementByID("loginButton");
            if (elSubmit != null)
            {
                elSubmit.ClickElement();
                return true;
            }

            return false;
        }

        #endregion


        #region Balance

        internal void CloseOpenedBalanceTab()
        {
            var elTaskBar = _wbMain.GetElementByIDEx("taskBar");
            if (elTaskBar != null)
            {
                var elBalanceTab = elTaskBar.GetElementByTag("div", "账户查询");
                if(elBalanceTab != null)
                {
                    elBalanceTab.NextSibling.ClickElement();
                }
            }
        }

        public bool NavigateMainMenu()
        {
            var elMenu = _wbMain.GetElementByTag("a", "首页");
            if (elMenu != null)
            {
                elMenu.ClickElement();
                return true;
            }

            return false;
        }

        internal bool NavigateMyAccountMenu()
        {
            var elMenu = _wbMain.GetElementByTag("a", "我的账户");
            if (elMenu != null)
            {
                elMenu.ClickElement();
                return true;
            }

            return false;
        }

        internal bool NavigateBalance()
        {
            var elMenu = _wbMain.GetElementByTag("a", "账户查询");
            if (elMenu != null)
            {
                elMenu.ClickElement();
                return true;
            }

            return false;
        }

        internal bool NavigateAccountStatusAndBalance(out string status, out Decimal balance)
        {
            status = "";
            balance = 0.00M;

            var elGridBox = _wbMain.GetElementByTagEx("div", "className", "accountGridBox");
            if (elGridBox != null)
            {
                var elStatusTitle = _wbMain.GetElementByTagEx("td", "账户状态：");
                if (elStatusTitle != null && elStatusTitle.NextSibling != null)
                {
                    status = elStatusTitle.NextSibling.GetData();
                }

                string webBalance = "";

                var elAmountTitle = _wbMain.GetElementByTagEx("td", "存款总额：");
                if (elAmountTitle != null && elAmountTitle.NextSibling != null)
                {
                    webBalance = elAmountTitle.NextSibling.GetData();
                }

                return Decimal.TryParse(webBalance, out balance);
            }

            return false;
        }

        #endregion


        #region Collection

        internal void CloseOpenedCollectionTab()
        {
            //System.Diagnostics.Debugger.Launch();
            var elTaskBar = _wbMain.GetElementByIDEx("taskBar");
            if (elTaskBar != null)
            {
                var elCollectionTab = elTaskBar.GetElementByTag("div", "活期交易明细查询");
                if (elCollectionTab != null)
                {
                    elCollectionTab.NextSibling.ClickElement();
                }

                elTaskBar.GetElementsByTagName("div")
                         .Cast<HtmlElement>()
                         .Where(x => x.GetAttribute("className") == "liana_taskbar_taskitem_close")
                         .ToList()
                         .ForEach(x => x.ClickElement());

                //var tabs = elTaskBar.GetElementsByTagName("div").Cast<HtmlElement>().Where(x => x.GetAttribute("className") == "liana_taskbar_taskitem_close").ToList();
                //foreach (var item in tabs)
                //{
                //    item.ClickElement();
                //}
            }
        }

        internal bool NavigateCollection()
        {
            var elMenu = _wbMain.GetElementByTag("a", "交易明细查询");
            if (elMenu != null)
            {
                elMenu.ClickElement();
                return true;
            }

            return false;
        }

        internal bool SelectDate(DateTime lastTime)
        {
            if (!_wbMain.GetData(_wbMain.Document.Body).Contains("活期交易明细查询"))
                return false;

            var elBeginDate = _wbMain.GetElementByIDEx("beginDate");
            if (elBeginDate != null &&
                elBeginDate.Document != null &&
                elBeginDate.Document.Body != null)
            {
                var elMainPanel = elBeginDate.Document.Body.GetElementByTag("div", "className", "main");
                if (elMainPanel != null)
                {
                    elBeginDate.Focus();
                    elBeginDate.SetAttribute("value", lastTime.ToString("yyyy-MM-dd"));

                    var elSearch = elMainPanel.GetElementByTag("a", "查询");
                    if (elSearch != null)
                    {
                        elSearch.ClickElement();

                        return true;
                    }
                }
            }

            return false;
        }

        internal bool CheckPaymentAmount(out bool sameAmount)
        {
            sameAmount = false;
            var elAmount = _wbMain.GetElementByIDEx("transferAmt");
            if(elAmount != null)
            {
                var webAmountData = elAmount.GetData();
                Decimal webAmount = 0M;
                Decimal.TryParse(webAmountData, out webAmount);
                sameAmount = webAmount == ModelData.Orders.FirstOrder.amount;
                return true;
            }

            return false;
        }

        internal bool GetCollectionList(out List<TransactionRecord> historyList, DateTime latestTime, out bool hasReachLatestTime, out Dictionary<string, HtmlElement> transactionWithDetails)
        {
            hasReachLatestTime = false;
            historyList = new List<TransactionRecord>();
            transactionWithDetails = new Dictionary<string, HtmlElement>();

            var elTableBody = _wbMain.GetElementByIDEx("resultTableBody");
            if (elTableBody != null)
            {
                foreach (HtmlElement elRow in elTableBody.GetElementsByTagName("tr"))
                {
                    if (elRow != null)
                    {
                        var date = elRow.Children[0].GetData().Replace("\r\n", " ");
                        var deposit = elRow.Children[3].GetData();
                        var withdraw = elRow.Children[4].GetData();
                        var balance = elRow.Children[5].GetData();

                        Decimal withdrawAmount = 0;
                        if(Decimal.TryParse(withdraw, out withdrawAmount) &&
                           withdrawAmount < 0)
                        {
                            deposit = Math.Abs(withdrawAmount).ToString();
                            withdraw = "-";
                        }

                        var amountData = deposit == "-" ? withdraw : deposit;

                        var transactionType = deposit == "-" ? 1 : 0;

                        if (transactionType == 1 && !amountData.Contains("-"))
                        {
                            amountData = $"-{amountData}";
                        }
                        
                        var transactionDateTime = DateTime.Parse(date);
                        var transactionDateTimeFormat = transactionDateTime.ToString("yyyy-MM-dd HH:mm:ss");

                        if (transactionDateTime <= latestTime)
                        {
                            hasReachLatestTime = true;
                            return true;
                        }

                        var historyDetails = new UAService.TransactionRecord()
                        {
                            transfer_to = ModelData.Card.AccountNumber,
                            trading_channel = elRow.Children[1].GetData(),
                            remarks = "",
                            transaction_time = transactionDateTimeFormat,
                            amount = decimal.Parse(amountData),
                            balance = decimal.Parse(balance),
                            payer = elRow.Children[6].GetData(),
                            transfer_from = elRow.Children[7].GetData(),
                            notes = elRow.Children[8].GetData(),
                            type = transactionType
                        };
                        
                        historyDetails.detail = $"{historyDetails.payer.NullOrWhitespaceAndTrim()}|^|" +
                                                $"{historyDetails.transfer_from.NullOrWhitespaceAndTrim()}|^|" +
                                                $"{historyDetails.transfer_to.NullOrWhitespaceAndTrim()}|^|" +
                                                $"{historyDetails.trading_channel.NullOrWhitespaceAndTrim()}|^|" +
                                                $"{historyDetails.amount.ToString("#,##0.00").NullOrWhitespaceAndTrim()}|^|" +
                                                $"{historyDetails.balance.ToString("#,##0.00").NullOrWhitespaceAndTrim()}|^|" +
                                                $"{historyDetails.remarks.NullOrWhitespaceAndTrim()}|^|" +
                                                $"{historyDetails.notes.NullOrWhitespaceAndTrim()}|^|" +
                                                $"{historyDetails.transaction_time}";


                        historyList.Add(historyDetails);

                        if((historyDetails.transfer_from != null &&
                            historyDetails.transfer_from.Length <= 11) ||
                           (historyDetails.payer != null &&
                            historyDetails.payer.Contains("支付宝"))
                           )
                        {
                            var elDetails = elRow.Children[9];
                            if (elDetails != null && 
                                elDetails.GetData().Contains("详情"))
                            {
                                transactionWithDetails.Add(historyDetails.detail, elDetails);
                            }
                        }
                    }
                }

                return true;
            }

            return false;
        }

        internal bool GetCollectionDetails(TransactionRecord data)
        {
            var elDetails = _wbMain.GetElementByIDEx("detailTable");

            if(elDetails != null)
            {
                var result = false;

                var elNotes = elDetails.GetElementByTag("td", "附言：");
                if(elNotes != null && 
                   elNotes.NextSibling != null)
                {
                    data.payer = elNotes.NextSibling.GetData();
                    result = true;
                }

                if (!result)
                {
                    var elRemarks = elDetails.GetElementByTag("td", "用途：");
                    if (elRemarks != null &&
                       elRemarks.NextSibling != null)
                    {
                        data.payer = elRemarks.NextSibling.GetData();
                        result = true;
                    }
                }

                if (result)
                {
                    var elClose = elDetails.Parent.GetElementByTag("a", "关闭");
                    if(elClose != null)
                    {
                        elClose.ClickElement();
                        return result;
                    }
                }
            }

            return false;
        }

        internal bool CheckWrongPassword(out string webError)
        {
            webError = "";

            var elContent = _wbMain.GetElementByTagEx("td", "className", "content");
            if(elContent != null)
            {
                webError = elContent.GetData();

                return true;
            }

            return false;
        }

        public bool GetTotalRows(out int total)
        {
            total = 0;

            var elTableFooter = _wbMain.GetElementByIDEx("tfootId");
            if (elTableFooter != null &&
                elTableFooter.Children.Count >= 1 &&
                elTableFooter.Children[0] != null &&
                elTableFooter.Children[0].Children.Count >= 2 &&
                elTableFooter.Children[0].Children[1] != null)
            {
                var webTotalRow = elTableFooter.Children[0].Children[1].GetData();
                if(int.TryParse(webTotalRow, out total))
                {
                    return true;
                }
            }

            return false;
        }

        internal bool NextPage()
        {
            var elContent = _wbMain.GetElementByIDEx("printContent");
            if (elContent != null)
            {
                var elPagination = elContent.GetElementByTag("div", "className", "turnpage");
                if(elPagination != null &&
                   elPagination.Children.Count >= 1 &&
                   elPagination.Children[0] != null)
                {
                    var elNext = elContent.GetElementByTag("div", "className", "nextPage");
                    if(elNext != null)
                    {
                        elNext.ClickElement();
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion


        #region Payment

        internal bool NavigateAutopayMenu()
        {
            var elMenu = _wbMain.GetElementByTag("a", "转账汇款");
            if (elMenu != null)
            {
                elMenu.ClickElement();
                return true;
            }

            return false;
        }

        internal bool NavigateAutopaySubMenu()
        {
            inputPaymentAction = InputPaymentAction.account_name;
            var elMenu = _wbMain.GetElementByTag("a", "一站式转账");
            if (elMenu != null)
            {
                elMenu.ClickElement();
                return true;
            }

            return false;
        }

        internal bool TransferPaymentInfo(out bool paymentpageNotLoaded)
        {
            paymentpageNotLoaded = false;
            var elSourceAccountNo = _wbMain.GetElementByIDEx("payAccount");
            if (elSourceAccountNo == null) return false;

            try
            {
                string accountNo = ModelData.Card.AccountNumber.Substring(0, 4) + "-" + ModelData.Card.AccountNumber.Substring(ModelData.Card.AccountNumber.Length - 4);
                string webAccountNo = elSourceAccountNo.GetData(ElementText.InnerText).Replace(" ", "");

                if (!String.IsNullOrEmpty(webAccountNo) &&
                    webAccountNo.Length >= 4)
                {
                    webAccountNo = webAccountNo.Substring(0, 4) + "-" + webAccountNo.Substring(webAccountNo.Length - 4);

                    if (accountNo != webAccountNo)
                        return false;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                paymentpageNotLoaded = true;
                return true;
            }

            return ModelData.Orders.FirstOrder.IsSameBank() ? SameBankTransfer() : CrossBankTransfer();
        }

        internal bool SubmitPaymentInfo()
        {
            var elAccountName = _wbMain.GetElementByIDEx("recAccountName");
            var elAccountNo = _wbMain.GetElementByIDEx("recAccount");

            if(elAccountName == null && elAccountNo == null)
            {
                return false;
            }

            var elSubmit = _wbMain.GetElementByIDEx("buttonNext");
            if(elSubmit != null)
            {
                elSubmit.ClickElement();
                return true;
            }

            return false;
        }

        internal bool TransferingPaymentInfo()
        {
            // TODO: Input Payment Verification
            var elSubmitPayment = _wbMain.GetElementByIDEx("buttonNext");
            //var elPaymentVerificationImage = _wbMain.GetElementByIDEx("_verifyImage");
            //var elPaymentVerificationCode = _wbMain.GetElementByIDEx("checkCode"); 

            var elToolUsb = _wbMain.GetElementByIDEx("securityToolUSB");
            if (elToolUsb != null && elSubmitPayment != null
                )
            {
                elToolUsb.ClickElement();
                elSubmitPayment.ClickElement();
                return true;
            }

            return false;
        }

        internal bool ChecCollectionError()
        {
            var classValue = new string[] { "errorBox", "errorTitle", "errorMsg" };
            var ukeyErrorList = new string[] { "queryOneBalanceList" };

            foreach (var item in classValue)
            {
                var elUkeyError = _wbMain.GetElementByTagEx("div", "className", item);
                var ukeyError = elUkeyError.GetData();

                if (elUkeyError != null)
                {
                    if (ukeyErrorList.Any(x => ukeyError.Contains(x)))
                    {
                        elUkeyError.GetElementByTag("button", "关闭").ClickElement();
                        return true;
                    }
                }
            }

            return false;
        }

        internal string CheckUkeyError()
        {
            var classValue = new string[] { "errorBox", "errorTitle", "errorMsg" };

            foreach (var item in classValue)
            {
                var elUkeyError = _wbMain.GetElementByTagEx("div", "className", item);
                var ukeyError = elUkeyError.GetData();

                var ukeyErrorList = new string[] { "未检测到Key盾",
                                               "The action was cancelled by the user.",
                                               "温馨提示"};

                if (elUkeyError != null)
                {
                    if (ukeyError.Contains("您已提交一笔相同的汇款"))
                    {
                        elUkeyError.GetElementByTag("button", "关闭").ClickElement();
                        return "";
                    }
                    else if (ukeyErrorList.Any(x => ukeyError.Contains(x)))
                    {
                        elUkeyError.GetElementByTag("button", "关闭").ClickElement();
                        return ukeyError;
                    }
                }
            }

            return "";
        }

        internal bool CheckPaymentError()
        {
            //var elPaymentError = _wbMain.GetElementByTagEx("div", "className", "fail");
            //var paymentError = elPaymentError.GetData();

            //var elResult = null;
            var paymentErrorList = new string[] { "EBLN5006", "EBPB0219" };
            var frames = _wbMain.Document.Window.Frames.OfType<HtmlWindow>().ToList();
            if (frames.Count > 0)
            {
                for (int i = 0; i <= frames.Count - 1; i++)
                {
                    //elResult = frames[i].GetElementByID(value);
                    //if (elResult != null)
                    //    return elResult;

                    var paymentError = frames[i].Document.Body.GetData();

                    //if (elPaymentError != null && paymentError.Contains("EBLN5006"))
                    if (paymentErrorList.Any(x => paymentError.Contains(x)))
                    {
                        //elPaymentError.GetElementByID("returnBtn").ClickElement();
                        return true;
                    }

                    var frame = frames[i].Document.Window.Frames.OfType<HtmlWindow>().ToList();
                    if (frame.Count > 0)
                    {
                        frames.AddRange(frame);
                    }
                }
            }

            

            return false;
        }

        internal bool ContainsUKeyDialog(params string[] titles)
        {
            Boolean result = false;
            try
            {
                foreach (string title in titles)
                {
                    IntPtr hwnd = FindWindow("#32770", title);
                    if (hwnd != IntPtr.Zero)
                    {
                        result = true;
                    }
                }
            }
            catch { }

            return result;
        }

        internal bool HasTransferResult()
        {
            var elSuccess = _wbMain.GetElementByIDEx("print");
            if (elSuccess != null)
                return true;

            var elFailed = _wbMain.GetElementByIDEx("fail");
            if (elFailed != null)
                return true;

            return false;
        }

        internal bool GetTransferredResult(out string notes)
        {
            notes = "";

            // Success
            var elTransferResult = _wbMain.GetElementByIDEx("print");
            //if (elTransferResult == null)
            //    return false;

            if(elTransferResult != null)
                notes = elTransferResult.GetData();

            if (String.IsNullOrEmpty(notes.Trim()))
            {
                // Failed
                var elFailed = _wbMain.GetElementByIDEx("fail");
                if (elFailed != null)
                    notes = elFailed.GetData();
            }

            //id print
            //div class sucess

            //success
            //id text_td 

            //details
            //div class detailTable

            return true;
        }

        private bool SameBankTransfer()
        {
            var elAccountName = _wbMain.GetElementByIDEx("recAccountName");
            var elAccountNo = _wbMain.GetElementByIDEx("recAccount");
            var elAmount = _wbMain.GetElementByIDEx("payAmount");
            var elSaveInfo = _wbMain.GetElementByIDEx("isSaveInfo");

            if (elAccountNo == null ||
                elAccountNo == null ||
                elAmount == null)
                return false;

            switch (inputPaymentAction)
            {
                case InputPaymentAction.account_name:
                    elAccountName.SetValue(ModelData.Orders.FirstOrder.account_name);
                    inputPaymentAction = InputPaymentAction.account_number;
                    break;
                case InputPaymentAction.account_number:
                    elAccountNo.Focus();
                    elAccountNo.SetValue("");
                    elAccountNo.SetValue(ModelData.Orders.FirstOrder.account_number);
                    elAccountNo.ClickElement();
                    SendKeys.Send("{TAB}");
                    inputPaymentAction = InputPaymentAction.amount;
                    break;
                case InputPaymentAction.amount:
                    elAmount.SetValue(ModelData.Orders.FirstOrder.amount.ToString());
                    elSaveInfo.ClickElement();
                    return true;
            }

            return false;
        }

        private bool CrossBankTransfer()
        {
            var elAccountName = _wbMain.GetElementByIDEx("recAccountName");
            var elAccountNo = _wbMain.GetElementByIDEx("recAccount");
            var elPayeeBank = _wbMain.GetElementByIDEx("recAccountOpenBank");
            var elTablePayeeBank = _wbMain.GetElementByIDEx("tr_openBank");
            var elAmount = _wbMain.GetElementByIDEx("payAmount");
            var elSaveInfo = _wbMain.GetElementByIDEx("isSaveInfo");

            if (elAccountNo == null ||
                elAccountNo == null ||
                elAmount == null)
                return false;

            if (elTablePayeeBank != null &&
                !elTablePayeeBank.ContainsStyle("display: none") &&
                elPayeeBank != null &&
                !(inputPaymentAction >= InputPaymentAction.account_number))
            {
                elPayeeBank.ClickElement();
                return false;
            }

            switch (inputPaymentAction)
            {
                case InputPaymentAction.account_name:
                    elAccountName.SetValue(ModelData.Orders.FirstOrder.account_name);
                    inputPaymentAction = InputPaymentAction.account_number;
                    break;
                case InputPaymentAction.account_number:
                    elAccountNo.Focus();
                    elAccountNo.SetValue("");
                    //elPayeeBank.Paste(ModelData.Orders.FirstOrder.issuing_bank);
                    elAccountNo.SetValue(ModelData.Orders.FirstOrder.account_number);
                    elAccountNo.ClickElement();
                    SendKeys.Send("{TAB}");
                    inputPaymentAction = InputPaymentAction.bank;
                    break;
                case InputPaymentAction.bank:
                    elPayeeBank.SetValue(ModelData.Orders.FirstOrder.issuing_bank);
                    inputPaymentAction = InputPaymentAction.amount;
                    break;
                case InputPaymentAction.amount:
                    elAmount.SetValue(ModelData.Orders.FirstOrder.amount.ToString());
                    elSaveInfo.ClickElement();
                    return true;
            }

            return false;
        }

        internal bool GetPaymentCaptchaText(out string captchaText)
        {
            captchaText = "";

            try
            {
                var elPaymentVerificationImage = _wbMain.GetElementByIDEx("_verifyImage");
                if (elPaymentVerificationImage == null)
                    return false;

                var captchaImageTry = 0;

                // Get Image From Browser
                Bitmap img = null;
                do
                {
                    img = GetPaymentCaptchaImage();
                    captchaImageTry++;

                    if (captchaImageTry >= 5)
                    {
                        RenewPaymentCaptcha();
                        return false;
                    }
                }
                while (img.IsNull());

                if (!img.IsNull())
                {
                    var captcha = SolvePaymentCaptcha(img);

                    if (!String.IsNullOrEmpty(captcha))
                        captcha = captcha.Replace(" ", "").Trim();

                    if (String.IsNullOrEmpty(captcha) ||
                        (!String.IsNullOrEmpty(captcha) && (captcha.Length < 4 || captcha.Length > 4)))
                    {
                        // Nothing
                    }
                    else
                    {
                        captchaText = captcha;
                        return true;
                    }
                }
            }
            catch (Exception ex) { }

            RenewPaymentCaptcha();

            return false;
        }

        internal bool CheckCollectionAccount(out bool result)
        {
            result = false;

            try
            {
                var elCollectionAccount = _wbMain.GetElementByIDEx("accNo");
                if (elCollectionAccount != null)
                {
                    string accountNo = ModelData.Card.AccountNumber.Substring(0, 4) + "-" + ModelData.Card.AccountNumber.Reverse().Substring(0, 4).Reverse();
                    string webAccountNo = elCollectionAccount.GetAttributeEx().Split('[')[0];

                    if (!String.IsNullOrEmpty(webAccountNo) &&
                        webAccountNo.Length >= 4)
                    {
                        webAccountNo = webAccountNo.Substring(0, 4) + "-" + webAccountNo.Reverse().Substring(0, 4).Reverse();
                        
                        result = accountNo == webAccountNo;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch { }

            return false;
        }

        private string SolvePaymentCaptcha(Bitmap image)
        {
            string destAccountNum = "0123456789";
            Bitmap imageClone = image.Clone(new Rectangle(0, 0, image.Width, image.Height), image.PixelFormat);
            return GetTextByAccountBased(imageClone, destAccountNum);
        }

        private void RenewPaymentCaptcha()
        {
            var elChangeCaptchaClick = _wbMain.GetElementByIDEx("_verifyImage");
            if (elChangeCaptchaClick != null)
            {
                elChangeCaptchaClick.ClickElement();
            }
        }

        private Bitmap GetPaymentCaptchaImage()
        {
            Bitmap image = null;
            var elPaymentVerificationImage = _wbMain.GetElementByIDEx("_verifyImage");
            IHTMLDocument2 doc = (IHTMLDocument2)elPaymentVerificationImage.Document.DomDocument;
            HtmlDocument d = _wbMain.Document;
            IHTMLControlRange imgRange1 = (IHTMLControlRange)((HTMLBody)doc.body).createControlRange();

            foreach (IHTMLImgElement img in doc.images)
            {
                imgRange1.add((IHTMLControlElement)img);
                object a = Clipboard.GetDataObject();

                imgRange1.execCommand("Copy", false, null);
                image = (Bitmap)Clipboard.GetDataObject().GetData(DataFormats.Bitmap);
                break;
            }

            return image;
        }

        #endregion

        private int getXoffset(HtmlElement el)
        {
            //get element pos
            int xPos = el.OffsetRectangle.Left;

            //get the parents pos
            HtmlElement tempEl = el.OffsetParent;
            while (tempEl != null)
            {
                xPos += tempEl.OffsetRectangle.Left;
                tempEl = tempEl.OffsetParent;
            }

            return xPos;
        }

        private int getYoffset(HtmlElement el)
        {
            //get element pos
            int yPos = el.OffsetRectangle.Top;

            //get the parents pos
            HtmlElement tempEl = el.OffsetParent;
            while (tempEl != null)
            {
                yPos += tempEl.OffsetRectangle.Top;
                tempEl = tempEl.OffsetParent;
            }

            return yPos;
        }
    }
}