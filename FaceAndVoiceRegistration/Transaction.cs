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
    class Transaction
    {
        SpeechRecognitionEngine recEngine;
        SpeechSynthesizer synthesizer;
        private SqlConnection conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\MK185299\Documents\Visual Studio 2017\Projects\FaceAndVoiceRegistrationSln\FaceAndVoiceRegistration\CustomerDatabase.mdf;Integrated Security=True");
        private SqlCommand cmd = new SqlCommand();
        private SqlDataReader dr;
        private int accountBal;
        private int accountNo;
        private Withdraw withdraw;

        public Transaction()
        {
            synthesizer = new SpeechSynthesizer();
            withdraw = new Withdraw();
        }

        public void DoTransaction(int accountNo)
        {
            try
            {
                synthesizer.SpeakAsync("Hello, Welcome to the new world of NCR Corporation. We are here to make your transaction better and secure.");
                synthesizer.SpeakAsync("Please listen the all instructions care fully to successfully complete your transaction process.");
                synthesizer.SpeakAsync("Say Withdraw to Withdraw your Amount.");
                synthesizer.SpeakAsync("Say Deposit to Deposit money in your account.");
                synthesizer.SpeakAsync("Say Check Balance to know the balance amount in your account.");
                synthesizer.SpeakAsync("Say done when you complete your transaction.");

                this.accountNo = accountNo;
                recEngine = new SpeechRecognitionEngine();
                Choices chList = new Choices();
                chList.Add(new string[] { "Withdraw", "Deposit", "Check Balance", "Done" });
                Grammar grammer = new Grammar(new GrammarBuilder(chList));
                recEngine.LoadGrammar(grammer);
                recEngine.SpeechRecognized += RecEngine_SpeechRecognized;
                recEngine.SetInputToDefaultAudioDevice();
                recEngine.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error : " + ex.Message);
            }
        }

        private void RecEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            try
            {
                switch(e.Result.Text.ToString())
                {
                    case "Withdraw":
                        recEngine.RecognizeAsyncStop();
                        withdraw.Withdrawal(accountNo);
                        break;

                    case "Deposit":
                        recEngine.RecognizeAsyncStop();
                        Task.Delay(2000);
                        synthesizer.SpeakAsync("To Deposit money. Say like Deposit and your amount how much you want to Deposit. Example if You want to Deposit 500 rupees then say Deposit 500.");
                        synthesizer.SpeakAsync("Hi Toshif. Now tell How much amount you want to deposit?");
                        break;

                    case "Check Balance":
                        recEngine.RecognizeAsyncStop();
                        accountBal = withdraw.AccountBalAfterWithdraw();
                        //CheckBalance();
                        synthesizer.SpeakAsync("You have ");
                        synthesizer.SpeakAsync(accountBal.ToString());
                        synthesizer.SpeakAsync("rupees balance in your account.");
                        break;

                    case "Done":
                        synthesizer.SpeakAsync("Thank you. your transaction is calceled");
                        recEngine.RecognizeAsyncStop();
                        synthesizer.Dispose();
                        Environment.Exit(0);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error :" + ex.Message);
            }
        }
    }
}
