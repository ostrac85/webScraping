using BankCardLib.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static CGB.Models.ModelData;

namespace CGB.UAService
{
    public static class BankCardService
    {
        private static string _Key = "6df03bc8d7d04348a61f8042b8d457d1";
        public static string hydraApiUrl = System.Configuration.ConfigurationManager.AppSettings["UAAPIUrl"];
        public static string defaultDays = System.Configuration.ConfigurationManager.AppSettings["DefaultQueryCollectionDays"];

        public static async Task<Card> GetCard(string cardCode)
        {
            var baseUri = BankCardConfiguration.GetAppSettings("UAAPIUrl") + $"hydra/api/get_bankcard_info?code={cardCode}";

            try
            {
                using (var client = new HttpClient())
                {
                    Task<HttpResponseMessage> response = client.GetAsync(new Uri(baseUri));
                    response.Wait();

                    var responseJson = await response.Result.Content.ReadAsStringAsync();

                    if (response.Result.IsSuccessStatusCode)
                    {
                        UAHyDraResult<Card> jsonCard = null;

                        jsonCard = JsonConvert.DeserializeObject<UAHyDraResult<Card>>(responseJson);
                        
                        if(jsonCard != null && jsonCard.Status == 0)
                        {
                            jsonCard.Data.AccountNumber = Models.Encryption.Encryption.Decrypt(jsonCard.Data.AccountNumber);

                            if (!String.IsNullOrEmpty(jsonCard.Data.LoginUsername))
                                jsonCard.Data.LoginUsername = Models.Encryption.Encryption.Decrypt(jsonCard.Data.LoginUsername);

                            if (!String.IsNullOrEmpty(jsonCard.Data.LoginPassword))
                                jsonCard.Data.LoginPassword = Models.Encryption.Encryption.Decrypt(jsonCard.Data.LoginPassword);

                            if (!String.IsNullOrEmpty(jsonCard.Data.PayPassword))
                                jsonCard.Data.PayPassword = Models.Encryption.Encryption.Decrypt(jsonCard.Data.PayPassword);

                            if (!String.IsNullOrEmpty(jsonCard.Data.UkeyPassword))
                                jsonCard.Data.UkeyPassword = Models.Encryption.Encryption.Decrypt(jsonCard.Data.UkeyPassword);
                            
                            return jsonCard.Data;
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show(jsonCard.Msg);
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task CollectionHistory(string cardCode, decimal balance, List<TransactionRecord> transactions, List<TempTransRecord> tempBankRecord)
        {
            var baseUri = BankCardConfiguration.GetAppSettings("UAAPIUrl") + $"hydra/api/upload_bankcard_new_record/";

            try
            {
                var transactionsFiltered = transactions.Distinct(new TransactionRecordEqualityComparer()).ToList();

                if (transactionsFiltered.Count == 0)
                    return;

                using (var client = new HttpClient())
                {
                    transactionsFiltered = CompareGetOrderID(transactionsFiltered, tempBankRecord, balance);

                    var balanceFormat = balance.ToString("#,##0.00");
                    var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    var signData = balanceFormat + cardCode + time + _Key;
                    
                    var sign = Sign.Compute(signData);

                    Transaction transaction = new Transaction
                    {
                        code = cardCode,
                        balance = balanceFormat,
                        time = time,
                        sign = sign.ToUpper(),
                        records = transactionsFiltered
                    };

                    var jsonString = JsonConvert.SerializeObject(transaction);
                    var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    Task<HttpResponseMessage> response = client.PostAsync(baseUri, content);
                    response.Wait();

                    var responseJson = await response.Result.Content.ReadAsStringAsync();

                    if (response.Result.IsSuccessStatusCode)
                    {
                    }
                    else
                    {
                        throw new Exception(responseJson);
                    }

                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
                throw ex;
            }
        }

        private static List<TransactionRecord> CompareGetOrderID(List<TransactionRecord> transactions, List<TempTransRecord> tempBankRecords, decimal balance)
        {
            List<TransactionRecord> updatedTransactions = new List<TransactionRecord>();
            try
            {
                if (tempBankRecords.Count > 0)
                {
                    foreach (TransactionRecord transaction in transactions)
                    {
                        var order_id = GetPaymentOrderID(transaction, tempBankRecords, balance);
                        if (!string.IsNullOrEmpty(order_id.ToString().Trim()))
                        {
                            transaction.order_id = order_id;
                        }
                        updatedTransactions.Add(transaction);
                    }
                    return updatedTransactions;
                }
                else
                {
                    return transactions;
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
                return transactions;
            }
        }


        private static long? GetPaymentOrderID(TransactionRecord transaction, List<TempTransRecord> tempBankRecords, decimal balance)
        {
            long? orderid = null;
            if (tempBankRecords.Count > 0)
            {
                foreach (TempTransRecord tempBankRecord in tempBankRecords)
                {
                    var account_number = "";
                    var account_last_digits = "";
                    var account_first_digits = "";
                    var transfer_from = "";

                    if (!string.IsNullOrEmpty(tempBankRecord.account_number))
                    {
                        account_number = tempBankRecord.account_number.ToString();
                        account_first_digits = account_number.Substring(0, 5).Replace(" ", "");
                        account_last_digits = tempBankRecord.account_number.Substring(account_number.Length - 4, 4).Replace(" ", "");
                    }

                    transfer_from = transaction.transfer_from.Replace(" ", "");
                    if (Math.Abs(transaction.amount) == Math.Abs(tempBankRecord.amount)
                        && transaction.payer == tempBankRecord.account_name
                        && transfer_from.Contains(account_last_digits)
                        && transfer_from.Contains(account_first_digits)
                        && (tempBankRecord.date_hour_mins == transaction.transaction_time.Substring(0, transaction.transaction_time.Length - 3)
                            )
                        )
                    {
                        orderid = tempBankRecord.order_id;
                    }
                    else if (Math.Abs(transaction.amount) == Math.Abs(tempBankRecord.amount)
                      && transaction.payer == tempBankRecord.account_name
                      && transfer_from.Contains(account_last_digits)
                      && transfer_from.Contains(account_first_digits)
                      && DateTime.Parse(transaction.transaction_time) >= DateTime.Parse(tempBankRecord.date_hour_mins)
                      && DateTime.Parse(tempBankRecord.date_hour_mins).AddMinutes(10) >= DateTime.Parse(transaction.transaction_time)
                      )
                    {
                        orderid = tempBankRecord.order_id;
                    }
                }
            }
            return orderid;
        }

        public static async Task<DateTime> GetLatestDate(string cardCode)
        {
            var baseUri = BankCardConfiguration.GetAppSettings("UAAPIUrl") + $"hydra/api/get_last_trans_info?code={cardCode}";
            DateTime? result = null;

            try
            {
                var forcedDateFilename = "ForcedDate";
                if (File.Exists(forcedDateFilename))
                {
                    var forcedDatedata = File.ReadAllText(forcedDateFilename).Trim();
                    DateTime forceDate;
                    if (DateTime.TryParse(forcedDatedata, out forceDate))
                    {
                        return forceDate;
                    }
                }

                using (var client = new HttpClient())
                {
                    Task<HttpResponseMessage> response = client.GetAsync(new Uri(baseUri));
                    response.Wait();

                    var responseJson = await response.Result.Content.ReadAsStringAsync();

                    if (response.Result.IsSuccessStatusCode)
                    {
                        UAHyDraResult<LastTransactionDateDetails> jsonCard = null;

                        jsonCard = JsonConvert.DeserializeObject<UAHyDraResult<LastTransactionDateDetails>>(responseJson);

                        if (jsonCard != null && jsonCard.Status == 0)
                            result = jsonCard.Data.transaction_time;
                        else
                        {
                            System.Windows.Forms.MessageBox.Show(jsonCard.Msg);
                            result = null;
                        }
                    }
                    else
                    {
                        result = null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if(result == null)
            {
                // Get Last 7 Days
                int defaultQueryCollectionDays = int.Parse(BankCardConfiguration.GetAppSettings("DefaultQueryCollectionDays"));
                result = DateTime.Now.AddDays(defaultQueryCollectionDays);
            }

            return (DateTime)result;
        }

        public static async Task SendSMSVerifyWarning(string cardCode)
        {
            
            try
            {
                var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var signData = cardCode + time + _Key;
                var sign = Sign.Compute(signData).ToUpper();
                var baseUri = BankCardConfiguration.GetAppSettings("UAAPIUrl") + $"hydra/api/sms_verify_warning/?code={cardCode}&time={HttpUtility.UrlEncode(time)}&sign={sign}";

                using (var client = new HttpClient())
                {
                    Task<HttpResponseMessage> response = client.GetAsync(new Uri(baseUri));
                    response.Wait();

                    var responseJson = await response.Result.Content.ReadAsStringAsync();

                    if (response.Result.IsSuccessStatusCode)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task SendCardFrozenWarning(string cardCode)
        {
            try
            {
                var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var signData = cardCode + time + _Key;
                var sign = Sign.Compute(signData).ToUpper();
                var baseUri = BankCardConfiguration.GetAppSettings("UAAPIUrl") + $"hydra/api/card_frozen_warning/?code={cardCode}&time={HttpUtility.UrlEncode(time)}&sign={sign}";

                using (var client = new HttpClient())
                {
                    Task<HttpResponseMessage> response = client.GetAsync(new Uri(baseUri));
                    response.Wait();

                    var responseJson = await response.Result.Content.ReadAsStringAsync();

                    if (response.Result.IsSuccessStatusCode)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task HeartBeat(string cardCode)
        {
            try
            {
                var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var signData = cardCode + time + _Key;
                var sign = Sign.Compute(signData).ToUpper();
                var baseUri = BankCardConfiguration.GetAppSettings("UAAPIUrl") + $"hydra/api/card_online_heart_beat/?code={cardCode}&time={HttpUtility.UrlEncode(time)}&sign={sign}";

                using (var client = new HttpClient())
                {
                    Task<HttpResponseMessage> response = client.GetAsync(new Uri(baseUri));
                    response.Wait();

                    var responseJson = await response.Result.Content.ReadAsStringAsync();
                    
                    if (response.Result.IsSuccessStatusCode)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<ExeWebSettings> GetBankSettings(string bank)
        {
            var baseUri = hydraApiUrl + $"hydra/api/get_exe_web_settings?bank={bank}";
            try
            {
                using (var client = new HttpClient())
                {
                    Task<HttpResponseMessage> response = client.GetAsync(new Uri(baseUri));
                    response.Wait();

                    var responseJson = await response.Result.Content.ReadAsStringAsync();
                    if (response.Result.IsSuccessStatusCode)
                    {
                        var item = JsonConvert.DeserializeObject<UAHyDraResult<ExeWebSettings>>(responseJson);
                        return item.Data;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task GetWithdrawOrdersForExe(int id)
        {
            var baseUri = hydraApiUrl + $"hydra/api/withdraw/get_orders_for_exe";
            try
            {
                using (var client = new HttpClient())
                {

                    var json = "{\"card_id\": " + id.ToString() + "}";

                    var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
                    Task<HttpResponseMessage> response = client.PostAsync(baseUri, stringContent);
                    response.Wait();

                    var responseJson = await response.Result.Content.ReadAsStringAsync();
                    if (response.Result.IsSuccessStatusCode)
                    {
                        UAHyDraResult<WithdrawOrders> jsonCardHistory = null;
                        jsonCardHistory = JsonConvert.DeserializeObject<UAHyDraResult<WithdrawOrders>>(responseJson);
                        if (jsonCardHistory.Data.list.Count > 0)
                        {
                            Orders.Withdrawals = jsonCardHistory.Data.list;
                        }
                        else
                        {
                            Orders.Withdrawals = new List<WithdrawOrder>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task SetExeLockOrder(long id, bool islocked)
        {
            var baseUri = hydraApiUrl + $"hydra/api/withdraw/exe_lock";
            try
            {
                using (var client = new HttpClient())
                {

                    var req = new RequestLock
                    {
                        order_id = id,
                        exe_locked = islocked
                    };

                    var json = JsonConvert.SerializeObject(req);

                    var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
                    Task<HttpResponseMessage> response = client.PostAsync(baseUri, stringContent);
                    response.Wait();

                    var responseJson = await response.Result.Content.ReadAsStringAsync();
                    if (response.Result.IsSuccessStatusCode)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task WithdrawEdit(long id, int status, string notes, DateTime operating_time)
        {
            var baseUri = hydraApiUrl + $"hydra/api/withdraw/edit";
            try
            {
                using (var client = new HttpClient())
                {
                    var req = new RequestWithdraw
                    {
                        order_id = id,
                        status = status,
                        notes = notes,
                        operating_time = operating_time,
                        updated_by = "Exe"
                    };

                    var json = JsonConvert.SerializeObject(req);
                    var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");

                    Task<HttpResponseMessage> response = client.PostAsync(baseUri, stringContent);
                    response.Wait();

                    var responseJson = await response.Result.Content.ReadAsStringAsync();
                    if (response.Result.IsSuccessStatusCode)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task ShowUkey(long order_id)
        {
            var baseUri = hydraApiUrl + $"hydra/api/withdraw/show_ukey";
            try
            {
                var request_data = new RequestUkey
                {
                    order_id = order_id,
                    show_ukey = true
                };
                using (var client = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(request_data);

                    var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
                    Task<HttpResponseMessage> response = client.PostAsync(baseUri, stringContent);
                    response.Wait();

                    var responseJson = await response.Result.Content.ReadAsStringAsync();
                    if (response.Result.IsSuccessStatusCode)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task SaveCardHistory(CardLoginHistory history)
        {
            var baseUri = hydraApiUrl + $"hydra/api/save_card_login_history";
            try
            {
                using (var client = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(history);

                    var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
                    Task<HttpResponseMessage> response = client.PostAsync(baseUri, stringContent);
                    response.Wait();

                    var responseJson = await response.Result.Content.ReadAsStringAsync();
                    if (response.Result.IsSuccessStatusCode)
                    {
                        UAHyDraResult<CardLoginHistory> jsonCardHistory = null;
                        jsonCardHistory = JsonConvert.DeserializeObject<UAHyDraResult<CardLoginHistory>>(responseJson);
                        Models.ModelData.CardLoginHistory = jsonCardHistory.Data;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<Boolean> PressUKey()
        {
            string presserApiUrl = System.Configuration.ConfigurationManager.AppSettings["PresserUrl"];
            var baseUri = presserApiUrl;

            try
            {
                using (var client = new HttpClient())
                {
                    Task<HttpResponseMessage> response = client.GetAsync(new Uri(baseUri));
                    response.Wait();

                    var responseJson = await response.Result.Content.ReadAsStringAsync();

                    if (response.Result.IsSuccessStatusCode)
                    {
                        Boolean jsonResult = false;

                        jsonResult = JsonConvert.DeserializeObject<Boolean>(responseJson);

                        return jsonResult;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task SendStatusNotification(string cardCode, string status)
        {
            try
            { 
                var baseUri = hydraApiUrl + $"hydra/api/send_card_status_warning?code={cardCode}&status={status}";

                using (var client = new HttpClient())
                {
                    Task<HttpResponseMessage> response = client.GetAsync(new Uri(baseUri));
                    response.Wait();

                    var responseJson = await response.Result.Content.ReadAsStringAsync();

                    if (response.Result.IsSuccessStatusCode)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}