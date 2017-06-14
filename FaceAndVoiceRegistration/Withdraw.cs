using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Data.SqlClient;
using System.Data;

namespace FaceAndVoiceRegistration
{
    class Withdraw
    {
        SpeechRecognitionEngine speechRecEngine3;
        SpeechRecognitionEngine speechrecEngine4;
        SpeechSynthesizer speechSynthesizer;

        private int requestAmount;
        private int accountBal;
        private SqlConnection conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\MK185299\Documents\Visual Studio 2017\Projects\FaceAndVoiceRegistrationSln\FaceAndVoiceRegistration\CustomerDatabase.mdf;Integrated Security=True");
        private SqlCommand cmd = new SqlCommand();
        private SqlDataReader dr;
        private int accountNo;

        public Withdraw()
        {
            speechSynthesizer = new SpeechSynthesizer();
        }

        public void Withdrawal(int accountNo)
        {
            try
            {
                this.accountNo = accountNo;
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select AccountBalance From AccountDetails where AccountNo = '" + accountNo + "'";
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        accountBal = dr.GetInt32(0);
                        Console.WriteLine("Account Balance is : " + accountBal);
                    }
                }
                conn.Close();
                
                
                speechSynthesizer.SpeakAsync("To withdraw money. Say like Withdraw and your amount how much you want to withdraw. Example if You want to withdraw 500 rupees then say Withdraw 500.");
                speechSynthesizer.SpeakAsync("Hello . Now tell me How much amount you want to withdraw?");
                speechrecEngine4 = new SpeechRecognitionEngine();
                Choices chList2 = new Choices();
                chList2.Add(new string[] { "Withdraw 100", "Withdraw 200", "Withdraw 300", "Withdraw 400", "Withdraw 500", "Withdraw 600", "Withdraw 700", "Withdraw 800", "Withdraw 900", "Withdraw 1000",
                                            "Withdraw 1100", "Withdraw 1200", "Withdraw 1300", "Withdraw 1400", "Withdraw 1500", "Withdraw 1600", "Withdraw 1700", "Withdraw 1800", "Withdraw 1900", "Withdraw 2000",
                                            "Withdraw 2100", "Withdraw 2200", "Withdraw 2300", "Withdraw 2400", "Withdraw 2500", "Withdraw 2600", "Withdraw 2700", "Withdraw 2800", "Withdraw 2900", "Withdraw 3000",
                                            "Withdraw 3100", "Withdraw 3200", "Withdraw 3300", "Withdraw 3400", "Withdraw 3500", "Withdraw 3600", "Withdraw 3700", "Withdraw 3800", "Withdraw 3900", "Withdraw 4000",
                                            "Withdraw 4100", "Withdraw 4200", "Withdraw 4300", "Withdraw 4400", "Withdraw 4500", "Withdraw 4600", "Withdraw 4700", "Withdraw 4800", "Withdraw 4900", "Withdraw 5000",
                                            "Withdraw 5100", "Withdraw 5200", "Withdraw 5300", "Withdraw 5400", "Withdraw 5500", "Withdraw 5600", "Withdraw 5700", "Withdraw 5800", "Withdraw 5900", "Withdraw 6000",
                                            "Withdraw 6100", "Withdraw 6200", "Withdraw 6300", "Withdraw 6400", "Withdraw 6500", "Withdraw 6600", "Withdraw 6700", "Withdraw 6800", "Withdraw 6900", "Withdraw 7000",
                                            "Withdraw 7100", "Withdraw 7200", "Withdraw 7300", "Withdraw 7400", "Withdraw 7500", "Withdraw 7600", "Withdraw 7700", "Withdraw 7800", "Withdraw 7900", "Withdraw 8000",
                                            "Withdraw 8100", "Withdraw 8200", "Withdraw 8300", "Withdraw 8400", "Withdraw 8500", "Withdraw 8600", "Withdraw 8700", "Withdraw 8800", "Withdraw 8900", "Withdraw 9000",
                                            "Withdraw 9100", "Withdraw 9200", "Withdraw 9300", "Withdraw 9400", "Withdraw 9500", "Withdraw 9600", "Withdraw 9700", "Withdraw 9800", "Withdraw 9900", "Withdraw 10000", "Cancel" });
                Grammar grammer2 = new Grammar(new GrammarBuilder(chList2));
                //speechrecEngine4.RequestRecognizerUpdate();
                speechrecEngine4.LoadGrammar(grammer2);
                speechrecEngine4.SpeechRecognized += SpeechrecEngine4_SpeechRecognized;
                speechrecEngine4.SetInputToDefaultAudioDevice();
                speechrecEngine4.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error : " + ex.Message);
            }
        }

        private void SpeechrecEngine4_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            try
            {
                switch (e.Result.Text.ToString())
                {
                    case "Withdraw 100":
                        Task.Delay(2000);
                        withdrawConfirmation(100);
                        break;

                    case "Withdraw 200":
                        Task.Delay(2000);
                        withdrawConfirmation(200);
                        break;

                    case "Withdraw 300":
                        Task.Delay(2000);
                        withdrawConfirmation(300);
                        break;

                    case "Withdraw 400":
                        Task.Delay(2000);
                        withdrawConfirmation(400);
                        break;

                    case "Withdraw 500":
                        speechSynthesizer.SpeakAsync("hello you are in.");
                        //Task.Delay(2000);
                        withdrawConfirmation(500);
                        break;

                    case "Withdraw 600":
                        Task.Delay(2000);
                        withdrawConfirmation(600);
                        break;

                    case "Withdraw 700":
                        Task.Delay(2000);
                        withdrawConfirmation(700);
                        break;

                    case "Withdraw 800":
                        Task.Delay(2000);
                        withdrawConfirmation(800);
                        break;

                    case "Withdraw 900":
                        Task.Delay(2000);
                        withdrawConfirmation(900);
                        break;

                    case "Withdraw 1000":
                        Task.Delay(2000);
                        withdrawConfirmation(1000);
                        break;

                    case "Withdraw 1100":
                        withdrawConfirmation(1100);
                        break;

                    case "Withdraw 1200":
                        withdrawConfirmation(1200);
                        break;

                    case "Withdraw 1300":
                        withdrawConfirmation(1300);
                        break;

                    case "Withdraw 1400":
                        withdrawConfirmation(1400);
                        break;

                    case "Withdraw 1500":
                        withdrawConfirmation(1500);
                        break;

                    case "Withdraw 1600":
                        withdrawConfirmation(1600);
                        break;

                    case "Withdraw 1700":
                        withdrawConfirmation(1700);
                        break;

                    case "Withdraw 1800":
                        withdrawConfirmation(1800);
                        break;

                    case "Withdraw 1900":
                        withdrawConfirmation(1900);
                        break;

                    case "Withdraw 2000":
                        withdrawConfirmation(2000);
                        break;

                    case "Withdraw 2100":
                        withdrawConfirmation(2100);
                        break;

                    case "Withdraw 2200":
                        withdrawConfirmation(2200);
                        break;

                    case "Withdraw 2300":
                        withdrawConfirmation(2300);
                        break;

                    case "Withdraw 2400":
                        withdrawConfirmation(2400);
                        break;

                    case "Withdraw 2500":
                        withdrawConfirmation(2500);
                        break;

                    case "Withdraw 2600":
                        withdrawConfirmation(2600);
                        break;

                    case "Withdraw 2700":
                        withdrawConfirmation(2700);
                        break;

                    case "Withdraw 2800":
                        withdrawConfirmation(2800);
                        break;

                    case "Withdraw 2900":
                        withdrawConfirmation(2900);
                        break;

                    case "Withdraw 3000":
                        withdrawConfirmation(3000);
                        break;

                    case "Withdraw 3100":
                        withdrawConfirmation(3100);
                        break;

                    case "Withdraw 3200":
                        withdrawConfirmation(3200);
                        break;

                    case "Withdraw 3300":
                        withdrawConfirmation(3300);
                        break;

                    case "Withdraw 3400":
                        withdrawConfirmation(3400);
                        break;

                    case "Withdraw 3500":
                        withdrawConfirmation(3500);
                        break;

                    case "Withdraw 3600":
                        withdrawConfirmation(3600);
                        break;

                    case "Withdraw 3700":
                        withdrawConfirmation(3700);
                        break;

                    case "Withdraw 3800":
                        withdrawConfirmation(3800);
                        break;

                    case "Withdraw 3900":
                        withdrawConfirmation(3900);
                        break;

                    case "Withdraw 4000":
                        withdrawConfirmation(4000);
                        break;

                    case "Withdraw 4100":
                        withdrawConfirmation(4100);
                        break;

                    case "Withdraw 4200":
                        withdrawConfirmation(4200);
                        break;

                    case "Withdraw 4300":
                        withdrawConfirmation(4300);
                        break;

                    case "Withdraw 4400":
                        withdrawConfirmation(4400);
                        break;

                    case "Withdraw 4500":
                        withdrawConfirmation(4500);
                        break;

                    case "Withdraw 4600":
                        withdrawConfirmation(4600);
                        break;

                    case "Withdraw 4700":
                        withdrawConfirmation(4700);
                        break;

                    case "Withdraw 4800":
                        withdrawConfirmation(4800);
                        break;

                    case "Withdraw 4900":
                        withdrawConfirmation(4900);
                        break;

                    case "Withdraw 5000":
                        withdrawConfirmation(5000);
                        break;

                    case "Withdraw 5100":
                        withdrawConfirmation(5100);
                        break;

                    case "Withdraw 5200":
                        withdrawConfirmation(5200);
                        break;

                    case "Withdraw 5300":
                        withdrawConfirmation(5300);
                        break;

                    case "Withdraw 5400":
                        withdrawConfirmation(5400);
                        break;

                    case "Withdraw 5500":
                        withdrawConfirmation(5500);
                        break;

                    case "Withdraw 5600":
                        withdrawConfirmation(5600);
                        break;

                    case "Withdraw 5700":
                        withdrawConfirmation(5700);
                        break;

                    case "Withdraw 5800":
                        withdrawConfirmation(5800);
                        break;

                    case "Withdraw 5900":
                        withdrawConfirmation(5900);
                        break;

                    case "Withdraw 6000":
                        withdrawConfirmation(6000);
                        break;

                    case "Withdraw 6100":
                        withdrawConfirmation(6100);
                        break;

                    case "Withdraw 6200":
                        withdrawConfirmation(6200);
                        break;

                    case "Withdraw 6300":
                        withdrawConfirmation(6300);
                        break;

                    case "Withdraw 6400":
                        withdrawConfirmation(6400);
                        break;

                    case "Withdraw 6500":
                        withdrawConfirmation(6500);
                        break;

                    case "Withdraw 6600":
                        withdrawConfirmation(6600);
                        break;

                    case "Withdraw 6700":
                        withdrawConfirmation(6700);
                        break;

                    case "Withdraw 6800":
                        withdrawConfirmation(6800);
                        break;

                    case "Withdraw 6900":
                        withdrawConfirmation(6900);
                        break;

                    case "Withdraw 7000":
                        withdrawConfirmation(7000);
                        break;

                    case "Withdraw 7100":
                        withdrawConfirmation(7100);
                        break;

                    case "Withdraw 7200":
                        withdrawConfirmation(7200);
                        break;

                    case "Withdraw 7300":
                        withdrawConfirmation(7300);
                        break;

                    case "Withdraw 7400":
                        withdrawConfirmation(7400);
                        break;

                    case "Withdraw 7500":
                        withdrawConfirmation(7500);
                        break;

                    case "Withdraw 7600":
                        withdrawConfirmation(7600);
                        break;

                    case "Withdraw 7700":
                        withdrawConfirmation(7700);
                        break;

                    case "Withdraw 7800":
                        withdrawConfirmation(7800);
                        break;

                    case "Withdraw 7900":
                        withdrawConfirmation(7900);
                        break;

                    case "Withdraw 8000":
                        withdrawConfirmation(8000);
                        break;

                    case "Withdraw 8100":
                        withdrawConfirmation(8100);
                        break;

                    case "Withdraw 8200":
                        withdrawConfirmation(8200);
                        break;

                    case "Withdraw 8300":
                        withdrawConfirmation(8300);
                        break;

                    case "Withdraw 8400":
                        withdrawConfirmation(8400);
                        break;

                    case "Withdraw 8500":
                        withdrawConfirmation(8500);
                        break;

                    case "Withdraw 8600":
                        withdrawConfirmation(8600);
                        break;

                    case "Withdraw 8700":
                        withdrawConfirmation(8700);
                        break;

                    case "Withdraw 8800":
                        withdrawConfirmation(8800);
                        break;

                    case "Withdraw 8900":
                        withdrawConfirmation(8900);
                        break;

                    case "Withdraw 9000":
                        withdrawConfirmation(9000);
                        break;

                    case "Withdraw 9100":
                        withdrawConfirmation(9100);
                        break;

                    case "Withdraw 9200":
                        withdrawConfirmation(9200);
                        break;

                    case "Withdraw 9300":
                        withdrawConfirmation(9300);
                        break;

                    case "Withdraw 9400":
                        withdrawConfirmation(9400);
                        break;


                    case "Withdraw 9500":
                        withdrawConfirmation(9500);
                        break;

                    case "Withdraw 9600":
                        withdrawConfirmation(9600);
                        break;

                    case "Withdraw 9700":
                        withdrawConfirmation(9700);
                        break;

                    case "Withdraw 9800":
                        withdrawConfirmation(9800);
                        break;

                    case "Withdraw 9900":
                        withdrawConfirmation(9900);
                        break;

                    case "Withdraw 10000":
                        withdrawConfirmation(10000);
                        break;

                    case "Cancel":
                        speechrecEngine4.RecognizeAsyncStop();
                        speechSynthesizer.SpeakAsync("Transaction canceled.");
                        Task.Delay(3000);
                        Environment.Exit(0);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error : " + ex.Message);
            }
            
        }
        

        private void withdrawConfirmation(int requestAmount)
        {
            this.requestAmount = requestAmount;
            speechrecEngine4.RecognizeAsyncStop();
            speechSynthesizer.SpeakAsync("Are you sure. You want to withdraw ");
            speechSynthesizer.SpeakAsync(this.requestAmount.ToString());
            speechSynthesizer.SpeakAsync("rupees?");
            speechSynthesizer.SpeakAsync("Say yes to confirm.");
            speechSynthesizer.SpeakAsync("Or No to cancel transaction.");
            speechSynthesizer.SpeakAsync("Say Response.");
            Task.Delay(2000);
            speechRecEngine3 = new SpeechRecognitionEngine();
            Choices chList3 = new Choices();
            chList3.Add(new string[] { "Yes", "No" });
            Grammar grammer3 = new Grammar(new GrammarBuilder(chList3));
            speechRecEngine3.RequestRecognizerUpdate();
            speechRecEngine3.LoadGrammar(grammer3);
            speechRecEngine3.SpeechRecognized += SpeechRecognitionEngine3_SpeechRecognized;
            speechRecEngine3.SetInputToDefaultAudioDevice();
            speechRecEngine3.RecognizeAsync(RecognizeMode.Multiple);
            Task.Delay(2000);
        }


        private void SpeechRecognitionEngine3_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Task.Delay(2000);
            try
            {
                switch (e.Result.Text.ToString())
                {
                    case "Yes":
                        //speechRecEngine3.RecognizeAsyncStop();
                        withdrawMoney(this.requestAmount);
                        break;

                    case "No":
                        //speechRecEngine3.RecognizeAsyncStop();
                        FinishTransaction();
                        break;

                }
            }
            catch (Exception ex)
            {

                Console.WriteLine("Error : ", ex.Message);
            }
        }

        private void withdrawMoney(int requestAmount)
        {
            try
            {
                speechRecEngine3.RecognizeAsyncStop();
                speechSynthesizer.SpeakAsync("Withdrawing.");
                Task.Delay(1000);
                if ((accountBal - requestAmount) <= 0)
                {
                    speechSynthesizer.SpeakAsync("Sorry. You don't have enough amount in your account to withdraw");
                    return;
                }
                else
                {
                    accountBal = (accountBal - requestAmount);
                    speechSynthesizer.SpeakAsync("You have withdraw ");
                    speechSynthesizer.SpeakAsync(this.requestAmount.ToString());
                    speechSynthesizer.SpeakAsync("rupees Successfully.");
                    conn.Open();
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "update AccountDetails set AccountBalance = '" + accountBal + "' where AccountNo = '" + accountNo + "'";
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    Console.WriteLine("Updated succesfully");
                    Task.Delay(2000);
                    return;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error : " + ex.Message);
            }
        }

        private void FinishTransaction()
        {
            speechRecEngine3.RecognizeAsyncStop();
            speechSynthesizer.SpeakAsync("Transaction canceled");
            Task.Delay(3000);
            Environment.Exit(0);
        }

        public int AccountBalAfterWithdraw()
        {
            return this.accountBal;
        }
    }
}
