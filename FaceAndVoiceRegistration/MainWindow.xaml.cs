using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.ProjectOxford.SpeakerRecognition;
using Microsoft.ProjectOxford.SpeakerRecognition.Contract;
using Microsoft.ProjectOxford.SpeakerRecognition.Contract.Identification;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Speech.Synthesis;
using System.Speech.Recognition;
using System.Data.SqlClient;
using System.Threading;

namespace FaceAndVoiceRegistration
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private webCam webcam;
        private string _selectedFile = "";
        private WaveIn _waveIn;
        private WaveFileWriter _fileWriter;
        private int photoNumCount = 0;
        private string userName = "";
        private string dirPath = "D:\\Pictures\\myncrgroup\\";
        private string userDirPath = "";
        private RegisterForm registerForm;
        //String GroupName = Guid.NewGuid().ToString();
        private readonly String GroupName = "myotherncrgroup";
        private Person p = new Person();
        private CreateProfileResponse creationResponse;
        private SortedList<Guid, string> enrollVoiceList = new SortedList<Guid, string>();
        private List<Guid> enrolllist = new List<Guid>();
        private SpeechRecognitionEngine speechRecEngine;
        private SpeechSynthesizer speechSynthesizer;
        private string faceIdentifiedUserName;
        private int accountBal;
        //private SpeechRecognitionEngine speechRecEngine2 = null;
        private Withdraw withdraw;
        private Transaction transaction;
        private int accountNo;
        private string faceid;
        private string voiceid;
        private SqlConnection conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\MK185299\Documents\Visual Studio 2017\Projects\FaceAndVoiceRegistrationSln\FaceAndVoiceRegistration\CustomerDatabase.mdf;Integrated Security=True");
        private SqlCommand cmd = new SqlCommand();
        private SqlDataReader dr;
        private string voiceIdentifiedUserName;
        private readonly string speakerAPISubscriptionKey = "d1947205f7df4a20aad03f1c55c63340";
        private readonly string faceAPISubscriptionKey = "404062c4115f475cbf6bbd574ed35001";


        public MainWindow()
        {
            InitializeComponent();
            InitializeRecorder();
            Task.Delay(3000);
            webcam = new webCam();
            webcam.InitializeWebCam(ref webImage);
            webcam.Start();
            Task.Delay(2000);
            InitializeRecorder();
            Task.Delay(3000);
            speechSynthesizer = new SpeechSynthesizer();
            withdraw = new Withdraw();
            transaction = new Transaction();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                speechSynthesizer.Rate = -2;
                speechSynthesizer.SpeakAsync("Hi Visitor, Welcome to the new world of NCR.");
                await Task.Delay(1000);
                speechSynthesizer.SpeakAsync("To verify your face. Please put your face clearly infront of the ATM.");
                //DeleteSpeaker();
                await UpdateAllSpeakersAsync();
                //DeleteEnrollment(Guid.Parse("b6dc2382-1657-4ffe-9092-f98a28509573"));
                SpeakerIdentificationServiceClient _serviceClient = new SpeakerIdentificationServiceClient(speakerAPISubscriptionKey);
                
                bool groupExists = false;

                var faceServiceClient = new FaceServiceClient(faceAPISubscriptionKey);
                // Test whether the group already exists
                try
                {

                    Title = String.Format("Request: Group {0} will be used to build a person database. Checking whether the group exists.", GroupName);
                    Console.WriteLine("Request: Group {0} will be used to build a person database. Checking whether the group exists.", GroupName);

                    await faceServiceClient.GetPersonGroupAsync(GroupName);
                    groupExists = true;
                    Title = String.Format("Response: Group {0} exists.", GroupName);
                    Console.WriteLine("Response: Group {0} exists.", GroupName);
                }
                catch (FaceAPIException ex)
                {
                    if (ex.ErrorCode != "PersonGroupNotFound")
                    {
                        Title = String.Format("Response: {0}. {1}", ex.ErrorCode, ex.ErrorMessage);
                        return;
                    }
                    else
                    {
                        Title = String.Format("Response: Group {0} did not exist previously.", GroupName);
                    }
                }

                if (groupExists)
                {
                    var cleanGroup = MessageBox.Show(string.Format("Requires a clean up for group \"{0}\" before setting up a new person database. Click OK to proceed, group \"{0}\" will be cleared.", GroupName), "Warning", MessageBoxButton.OKCancel);
                    if (cleanGroup == MessageBoxResult.OK)
                    {
                        await faceServiceClient.DeletePersonGroupAsync(GroupName);
                    }
                    else
                    {
                        return;
                    }
                }

                Title = String.Format("Request: Creating group \"{0}\"", GroupName);
                try
                {
                    await faceServiceClient.CreatePersonGroupAsync(GroupName, GroupName);
                    Title = String.Format("Response: Success. Group \"{0}\" created", GroupName);
                }
                catch (FaceAPIException ex)
                {
                    Title = String.Format("Response: {0}. {1}", ex.ErrorCode, ex.ErrorMessage);
                    return;
                }
                //DeleteSpeaker();
                //DeleteEnrollment(Guid.Parse("b6dc2382-1657-4ffe-9092-f98a28509573"));
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error : ", ex.Message);
            }
        }

        /// <summary>
        /// Initialize NAudio recorder instance
        /// </summary>
        private void InitializeRecorder()
        {
            _waveIn = new WaveIn();
            _waveIn.DeviceNumber = 0;
            int sampleRate = 16000; // 16 kHz
            int channels = 1; // mono
            _waveIn.WaveFormat = new WaveFormat(sampleRate, channels);
            _waveIn.DataAvailable += waveIn_DataAvailable;
            _waveIn.RecordingStopped += waveSource_RecordingStopped;
        }

        /// <summary>
        /// A method that's called whenever there's a chunk of audio is recorded
        /// </summary>
        /// <param name="sender">The sender object responsible for the event</param>
        /// <param name="e">The arguments of the event object</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (_fileWriter == null)
            {
                Console.WriteLine("Error");
            }
            _fileWriter.Write(e.Buffer, 0, e.BytesRecorded);
        }

        /// <summary>
        /// A listener called when the recording stops
        /// </summary>
        /// <param name="sender">Sender object responsible for event</param>
        /// <param name="e">A set of arguments sent to the listener</param>
        private void waveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            _fileWriter.Dispose();
            _fileWriter = null;

            //Dispose recorder object
            _waveIn.Dispose();
            InitializeRecorder();

        }
        
        /// <summary>
        /// Adds a speaker profile to the speakers list
        /// </summary>
        /// <param name="speaker">The speaker profile to add</param>
        public void AddSpeaker(Profile speaker)
        {
            Console.WriteLine("Adding profile...");
            Console.WriteLine("Another Id is : {0}", Guid.Parse(speaker.ProfileId.ToString()));
            enrolllist.Add(Guid.Parse(speaker.ProfileId.ToString()));
            Console.WriteLine("Done..");
        }
        
        /// <summary>
        /// Retrieves all the speakers asynchronously and adds them to the list
        /// </summary>
        /// <returns>Task to track the status of the asynchronous task.</returns>
        public async Task UpdateAllSpeakersAsync()
        {
            try
            {
                SpeakerIdentificationServiceClient _serviceClient = new SpeakerIdentificationServiceClient(speakerAPISubscriptionKey);
                Title = String.Format("Retrieving All Profiles...");
                Profile[] allProfiles = await _serviceClient.GetProfilesAsync();
                Title = String.Format("All Profiles Retrieved.");
                enrollVoiceList.Clear();
                foreach (Profile profile in allProfiles)
                     AddSpeaker(profile);
            }
            catch (GetProfileException ex)
            {
                Console.WriteLine("Error Retrieving Profiles: " + ex.Message);
                GC.Collect();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                GC.Collect();
            }
        }


        //To delete the speaker profile.
        private async void DeleteSpeaker()
        {
            try
            {
                SpeakerIdentificationServiceClient _serviceClient = new SpeakerIdentificationServiceClient(speakerAPISubscriptionKey);
                Title = String.Format("Deleting All Profiles...");
                Profile[] allProfiles = await _serviceClient.GetProfilesAsync();
                Title = String.Format("All Profiles Deleted.");
                int i = 0;
                foreach (Profile profile in allProfiles)
                {
                    i++;
                    Delete(profile);
                    if (i > 5)
                    {
                        return;
                    }
                }
                GC.Collect();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error : " + ex.Message);
                GC.Collect();
            }
        }

        //Delete the speaker profiles
        private async void Delete(Profile speaker)
        {
            try
            {
                SpeakerIdentificationServiceClient _serviceClient = new SpeakerIdentificationServiceClient(speakerAPISubscriptionKey);
                Console.WriteLine("Deleting profile...");
                Console.WriteLine("Another Id is : {0}", Guid.Parse(speaker.ProfileId.ToString()));
                await _serviceClient.DeleteProfileAsync(Guid.Parse(speaker.ProfileId.ToString()));
                Console.WriteLine("Deleted...");
                GC.Collect();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error : " + ex.Message);
                GC.Collect();
            }
        }


        private async void DeleteEnrollment(Guid speakerid)
        {
            try
            {
                Console.WriteLine("In");
                SpeakerIdentificationServiceClient _serviceClient = new SpeakerIdentificationServiceClient(speakerAPISubscriptionKey);
                await _serviceClient.ResetEnrollmentsAsync(speakerid);
                Console.WriteLine("Done");
            }
            catch (Exception ex)
            {

                Console.WriteLine("Error : " + ex.Message);
            }
        }

        //To make the user's profile. 
        private async void registerBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                registerBtn.IsEnabled = false;
                var faceServiceClient = new FaceServiceClient(faceAPISubscriptionKey);
                registerForm = new RegisterForm();

                registerForm.ShowDialog();
                
                if(registerForm.textBox.Text == "")
                {
                    speechSynthesizer.SpeakAsync("Please enter your name.");
                    var result = MessageBox.Show("Please Enter your Name.");
                    if (result == MessageBoxResult.OK)
                    {
                        registerForm = new RegisterForm();
                        registerForm.ShowDialog();
                    }
                    registerBtn.IsEnabled = true;
                    return;
                }
                else
                {
                    userName = registerForm.textBox.Text;
                    userDirPath = dirPath + userName + "\\";
                    bool exist = Directory.Exists(dirPath + userName);

                    if (!exist)
                    {
                        Directory.CreateDirectory(dirPath + userName);
                        accountBal = 10000;

                        //Insert some amount in database along with username.
                        conn.Open();
                        SqlCommand cmd = conn.CreateCommand();
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.CommandText = "Insert into AccountDetails (AccountBalance,CustomerName) values ('" + accountBal + "' , '" + userName + "')";
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        Console.WriteLine("Record Successfully Inserted");

                        //Fetch the accountno from AccountDetails by using username.
                        conn.Open();
                        SqlCommand cmdd = conn.CreateCommand();
                        cmdd.CommandType = System.Data.CommandType.Text;
                        cmdd.CommandText = "Select AccountNo From AccountDetails where CustomerName = '" + userName + "'";
                        dr = cmdd.ExecuteReader();
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                accountNo = int.Parse(dr[0].ToString());
                            }
                        }
                        dr.Close();

                        //Insert this accountno into AuthenticationDetails table.
                        SqlCommand cmd1 = conn.CreateCommand();
                        cmd1.CommandType = System.Data.CommandType.Text;
                        cmd1.CommandText = "Insert into AuthenticationDetails (AccountNo) values ('" + accountNo + "')";
                        cmd1.ExecuteNonQuery();
                        conn.Close();
                        Console.WriteLine("Record Successfully Inserted");

                        await Task.Delay(2000);
                        speechSynthesizer.SpeakAsync("Ok, Now you need to capture your three photos.");
                        Title = string.Format("Ready...Now click your three photos.");
                    }
                    else
                    {
                        registerBtn.IsEnabled = true;
                        SqlCommand cmd4 = conn.CreateCommand();
                        cmd4.CommandType = System.Data.CommandType.Text;
                        cmd4.CommandText = "Select * From AuthenticationDetails where FaceId = '" + faceid + "'";
                        dr = cmd.ExecuteReader();
                        if(dr.Read())
                        {
                            speechSynthesizer.SpeakAsync("You already registered your data.");
                            Title = string.Format("");
                            return;
                        }
                        else
                        {
                            Console.WriteLine("Sorry try with another name");
                            speechSynthesizer.SpeakAsync("Try with another Name.");
                            registerBtn.IsEnabled = true;
                            return;
                        }
                    }
                }
                //creating speaker profile with en-us locality.
                SpeakerIdentificationServiceClient _serviceClient = new SpeakerIdentificationServiceClient(speakerAPISubscriptionKey);
                creationResponse = await _serviceClient.CreateProfileAsync("en-US");
                var tag = System.IO.Path.GetFileName(dirPath + userName);
                Console.WriteLine("FileName : {0}", tag);
                p.PersonName = tag;
                // Call create person REST API, the new create person id will be returned
                Console.WriteLine("Request: Creating person \"{0}\"", p.PersonName);
                p.PersonId = (await faceServiceClient.CreatePersonAsync(GroupName, p.PersonName)).PersonId.ToString();
                Console.WriteLine("Person Id is : {0}", p.PersonId);
                faceid = p.PersonId.ToString();
                Console.WriteLine("Response: Success. Person \"{0}\" (PersonID:{1}) created", p.PersonName, p.PersonId.ToString());
                await Task.Delay(3000);
                captureBtn.IsEnabled = true;
                GC.Collect();
            }
            catch (Exception ex)
            {
                registerBtn.IsEnabled = true;
                MessageBox.Show(ex.Message);

            } 
        }

        private void captureBtn_Click(object sender, RoutedEventArgs e)
        {
            captureImage.Source = webImage.Source;
            saveImageBtn.IsEnabled = true;
        }

        private async void saveImgBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var faceServiceClient = new FaceServiceClient(faceAPISubscriptionKey);
                if (photoNumCount <= 2 && photoNumCount >= 0)
                {
                    Helper.SaveImageCapture((BitmapSource)captureImage.Source, userDirPath, userName + photoNumCount);
                    
                    var imgPath = userDirPath + userName + photoNumCount + ".jpg";
                    using (var fStream = File.OpenRead(imgPath))
                    {
                        var persistFace = await faceServiceClient.AddPersonFaceAsync(GroupName, Guid.Parse(p.PersonId), fStream, imgPath);
                        Title = String.Format("{0} Face Added to API", photoNumCount);
                    }
                    photoNumCount++;
                    speechSynthesizer.SpeakAsync("Successful registered.");
                    speechSynthesizer.SpeakAsync(photoNumCount.ToString());
                    speechSynthesizer.SpeakAsync("Photo. Now you have to registered.");
                    speechSynthesizer.SpeakAsync((3 - photoNumCount).ToString());
                    speechSynthesizer.SpeakAsync("More photo");
                    Title = string.Format("Successful registered {0} photo. Now you have to registered {1} more photo.", photoNumCount, (3 - photoNumCount));
                    
                }
                else
                {
                    photoNumCount = 0;
                    conn.Open();
                    SqlCommand cmd1 = conn.CreateCommand();
                    cmd1.CommandType = System.Data.CommandType.Text;
                    cmd1.CommandText = "update AuthenticationDetails set FaceId = '" + faceid + "' where AccountNo = '" + accountNo + "'";
                    cmd1.ExecuteNonQuery();
                    conn.Close();
                    MessageBox.Show("Record Updated Successfully");
                    Title = string.Format("You have saved three photos....");
                    speechSynthesizer.SpeakAsync("Thank you, You have successfully registered your face.");
                    speechSynthesizer.SpeakAsync("Now you need to registered your voice.");
                    speechSynthesizer.SpeakAsync("To registered your voice say like.");
                    speechSynthesizer.SpeakAsync("My voice is stronger than my password. Verify my voice.");
                    speechSynthesizer.SpeakAsync("Now ready to speak.");
                    await Task.Delay(4000);
                    recordBtn.IsEnabled = true;
                    captureBtn.IsEnabled = false;
                    saveImageBtn.IsEnabled = false;
                    return;
                }
                GC.Collect();
            }
            catch(Exception ex)
            {
                captureBtn.IsEnabled = true;
                Console.WriteLine(ex.Message);
                GC.Collect();
            }
        }

        
        private void recordBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (WaveIn.DeviceCount == 0)
                {
                    throw new Exception("Cannot detect microphone.");
                }

                _selectedFile = userDirPath + userName + ".wav";

                bool exist = File.Exists(_selectedFile);
                if (!exist)
                {

                    _fileWriter = new NAudio.Wave.WaveFileWriter(_selectedFile, _waveIn.WaveFormat);
                    _waveIn.StartRecording();

                    Title = String.Format("Recording...");
                    recordBtn.IsEnabled = false;
                    stopRecordBtn.IsEnabled = true;

                }
                else
                {
                    MessageBox.Show("You have already registered your voice.");
                    faceIdentifyBtn.IsEnabled = true;
                }
                GC.Collect();
            }
            catch (Exception ex)
            {
                recordBtn.IsEnabled = true;
                Console.WriteLine("Error: " + ex.Message);
                GC.Collect();
            }
            
        }

        private async void stopRecordBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Guid aakashId = Guid.Parse("b6dc2382-1657-4ffe-9092-f98a28509573");
                Title = string.Format("Stopping.....");
                SpeakerIdentificationServiceClient _serviceClient = new SpeakerIdentificationServiceClient(speakerAPISubscriptionKey);
                if (_waveIn != null)
                {
                    _waveIn.StopRecording();
                    recordBtn.IsEnabled = false;
                    stopRecordBtn.IsEnabled = false;
                    Title = String.Format("Recording Stopped");
                    speechSynthesizer.SpeakAsync("Wait we are registering your voice.");
                    await Task.Delay(5000);
                    Console.WriteLine("Speaker Id : {0}", creationResponse.ProfileId);
                    Title = String.Format("Enrolling....");
                    try
                    {
                        if (_selectedFile == "")
                            throw new Exception("No File Selected.");
                        Title = String.Format("Enrolling Speaker...");

                        OperationLocation processPollingLocation;
                        using (Stream audioStream = File.OpenRead(_selectedFile))
                        {
                            processPollingLocation = await _serviceClient.EnrollAsync(audioStream, creationResponse.ProfileId, true);
                            //processPollingLocation = await _serviceClient.EnrollAsync(audioStream, aakashId, true);
                        }

                        EnrollmentOperation enrollmentResult;
                        int numOfRetries = 10;
                        TimeSpan timeBetweenRetries = TimeSpan.FromSeconds(5.0);
                        while (numOfRetries > 0)
                        {
                            await Task.Delay(timeBetweenRetries);
                            enrollmentResult = await _serviceClient.CheckEnrollmentStatusAsync(processPollingLocation);

                            if (enrollmentResult.Status == Microsoft.ProjectOxford.SpeakerRecognition.Contract.Identification.Status.Succeeded)
                            {
                                break;
                            }
                            else if (enrollmentResult.Status == Microsoft.ProjectOxford.SpeakerRecognition.Contract.Identification.Status.Failed)
                            {
                                
                                throw new EnrollmentException(enrollmentResult.Message);
                            }
                            numOfRetries--;
                        }
                        if (numOfRetries <= 0)
                        {
                            throw new EnrollmentException("Enrollment operation timeout.");
                        }

                        Console.WriteLine("Guid is : {0}", creationResponse.ProfileId);
                        voiceid = creationResponse.ProfileId.ToString();

                        conn.Open();
                        SqlCommand cmd = conn.CreateCommand();
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.CommandText = "update AuthenticationDetails set VoiceId = '" + voiceid + "' where AccountNo = '" + accountNo + "'";
                        cmd.ExecuteNonQuery();
                        conn.Close();

                        MessageBox.Show("Record Updated Successfully.");

                        enrollVoiceList.Add(Guid.Parse(creationResponse.ProfileId.ToString()), userName);
                        speechSynthesizer.SpeakAsync("Thank you. You have successfully registered your voice.");
                        speechSynthesizer.SpeakAsync("From now onward you can use your voice to make your transactions secure.");
                        faceIdentifyBtn.IsEnabled = true;
                        Title = String.Format("Enrollment Done");
                        Console.WriteLine("Enrollment Done");
                        //window.Log("Enrollment Done.");
                        await UpdateAllSpeakersAsync();
                        GC.Collect();
                    }
                    catch (EnrollmentException ex)
                    {
                        //window.Log("Enrollment Error: " + ex.Message);
                        //File.Delete(_selectedFile);
                        recordBtn.IsEnabled = true;
                        stopRecordBtn.IsEnabled = false;
                        speechSynthesizer.SpeakAsync("Sorry, Try again.");
                        Title = String.Format("Enrollment Error: " + ex.Message);
                        GC.Collect();
                    }
                    catch (Exception ex)
                    {
                        //File.Delete(_selectedFile);
                        recordBtn.IsEnabled = true;
                        stopRecordBtn.IsEnabled = false;
                        //window.Log("Error: " + ex.Message);
                        Title = String.Format("Error: " + ex.Message);
                        GC.Collect();
                    }
                }
                GC.Collect();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                GC.Collect();
            }
        }

        private async void faceIdentifyBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                faceIdentifyBtn.IsEnabled = false;
                //capture photo than save.
                captureImage.Source = webImage.Source;
                Helper.SaveImageCapture((BitmapSource)captureImage.Source);

                string getDirectory = Directory.GetCurrentDirectory();
                string filePath = getDirectory + "\\test1.jpg";

                System.Drawing.Image image1 = System.Drawing.Image.FromFile(filePath);

                var faceServiceClient = new FaceServiceClient(faceAPISubscriptionKey);
                try
                {
                    Title = String.Format("Request: Training group \"{0}\"", GroupName);
                    await faceServiceClient.TrainPersonGroupAsync(GroupName);

                    TrainingStatus trainingStatus = null;
                    while (true)
                    {
                        await Task.Delay(1000);
                        trainingStatus = await faceServiceClient.GetPersonGroupTrainingStatusAsync(GroupName);
                        Title = String.Format("Response: {0}. Group \"{1}\" training process is {2}", "Success", GroupName, trainingStatus.Status);
                        if (trainingStatus.Status.ToString() != "running")
                        {
                            break;
                        }
                    }
                }
                catch (FaceAPIException ex)
                {

                    Title = String.Format("Response: {0}. {1}", ex.ErrorCode, ex.ErrorMessage);
                    faceIdentifyBtn.IsEnabled = true;
                }

                Title = "Detecting....";

                using (Stream s = File.OpenRead(filePath))
                {
                    var faces = await faceServiceClient.DetectAsync(s);
                    var faceIds = faces.Select(face => face.FaceId).ToArray();

                    var faceRects = faces.Select(face => face.FaceRectangle);
                    FaceRectangle[] faceRect = faceRects.ToArray();
                    if (faceRect.Length == 1)
                    {
                        Title = String.Format("Detection Finished. {0} face(s) detected", faceRect.Length);
                        speechSynthesizer.SpeakAsync("We have detected.");
                        speechSynthesizer.SpeakAsync(faceRect.Length.ToString());
                        speechSynthesizer.SpeakAsync("face.");
                        speechSynthesizer.SpeakAsync("Please Wait we are identifying your face.");

                        await Task.Delay(3000);
                        Title = "Identifying.....";
                        try
                        {
                            Console.WriteLine("Group Name is : {0}, faceIds is : {1}", GroupName, faceIds);
                            var results = await faceServiceClient.IdentifyAsync(GroupName, faceIds);

                            foreach (var identifyResult in results)
                            {
                                Title = String.Format("Result of face: {0}", identifyResult.FaceId);

                                if (identifyResult.Candidates.Length == 0)
                                {
                                    Title = String.Format("No one identified");
                                    MessageBox.Show("Hi, Make sure you have registered your face. Try to register now.");
                                    speechSynthesizer.SpeakAsync("Sorry. No one identified.");
                                    speechSynthesizer.SpeakAsync("Please make sure you have previously registered your face with us.");
                                    registerBtn.IsEnabled = true;
                                    faceIdentifyBtn.IsEnabled = false;
                                    return;
                                }
                                else
                                {
                                    // Get top 1 among all candidates returned
                                    var candidateId = identifyResult.Candidates[0].PersonId;
                                    var person = await faceServiceClient.GetPersonAsync(GroupName, candidateId);
                                    faceIdentifiedUserName = person.Name.ToString();
                                    Title = String.Format("Identified as {0}", person.Name);
                                    speechSynthesizer.SpeakAsync("Hi.");
                                    speechSynthesizer.Speak(person.Name.ToString());
                                    speechSynthesizer.SpeakAsync("Now you need to verify your voice.");
                                    speechSynthesizer.SpeakAsync("To verify your voice. Say like that.");
                                    speechSynthesizer.SpeakAsync("My voice is stronger than my password. Verify my voice.");
                                    faceIdentifyBtn.IsEnabled = false;
                                    identifyRecord.IsEnabled = true;
                                }
                            }
                            GC.Collect();
                        }
                        catch (FaceAPIException ex)
                        {
                            Title = String.Format("Failed...Try Again.");
                            speechSynthesizer.SpeakAsync("First register your face.");
                            Console.WriteLine("Error : {0} ", ex.Message);
                            image1.Dispose();
                            File.Delete(filePath);
                            GC.Collect();
                            registerBtn.IsEnabled = true;
                            return;
                        }
                    }
                    else if(faceRect.Length >1)
                    {
                        Title = String.Format("More than one faces detected. Make sure only one face is in the photo. Try again..");
                        speechSynthesizer.SpeakAsync("More than one faces detected. Make sure only one face is in the photo. Try again..");
                        faceIdentifyBtn.IsEnabled = true;
                        return;
                    }
                    else
                    {
                        Title = String.Format("No one detected in the photo. Please make sure your face is infront of the webcam. Try again with the correct photo.");
                        speechSynthesizer.SpeakAsync("No one detected. Please make sure your face is infront of the webcam. Try again with the correct photo.");
                        faceIdentifyBtn.IsEnabled = true;
                        return;
                    }

                    image1.Dispose();
                    File.Delete(filePath);
                    GC.Collect();
                    await Task.Delay(2000);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error : ", ex.Message);
                faceIdentifyBtn.IsEnabled = true;
                GC.Collect();
            }
        }

        private void identifyRecord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (WaveIn.DeviceCount == 0)
                {
                    throw new Exception("Cannot detect microphone.");
                }
                identifyRecord.IsEnabled = false;
                voiceIdentifyBtn.IsEnabled = true;

                //save file.
                string getDirectory = Directory.GetCurrentDirectory();
                _selectedFile = getDirectory + "\\Sample2.wav";
                _fileWriter = new NAudio.Wave.WaveFileWriter(_selectedFile, _waveIn.WaveFormat);
                _waveIn.StartRecording();

                Title = String.Format("Recording...");
                GC.Collect();
            }
            catch (Exception ge)
            {
                Console.WriteLine("Error: " + ge.Message);
                identifyRecord.IsEnabled = true;
                GC.Collect();
            }
        }

        private async void voiceIdentifyBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                voiceIdentifyBtn.IsEnabled = false;
                _identificationResultStckPnl.Visibility = Visibility.Hidden;
                if (_waveIn != null)
                {
                    _waveIn.StopRecording();
                }

                TimeSpan timeBetweenSaveAndIdentify = TimeSpan.FromSeconds(5.0);
                await Task.Delay(timeBetweenSaveAndIdentify);

                SpeakerIdentificationServiceClient _serviceClient = new SpeakerIdentificationServiceClient(speakerAPISubscriptionKey);

                List<Guid> list = new List<Guid>();
                Profile[] allProfiles = await _serviceClient.GetProfilesAsync();
                int itemsCount = 0;
                foreach (Profile profile in allProfiles)
                {
                    list.Add(profile.ProfileId);
                    itemsCount++;
                }
                Guid[] selectedIds = new Guid[itemsCount];
                for (int i = 0; i < itemsCount; i++)
                {
                    selectedIds[i] = list[i];
                }
                if (_selectedFile == "")
                    throw new Exception("No File Selected.");

                speechSynthesizer.SpeakAsync("Please wait we are verifying your voice.");
                Title = String.Format("Identifying File...");
                OperationLocation processPollingLocation;
                Console.WriteLine("Selected file is : {0}", _selectedFile);
                using (Stream audioStream = File.OpenRead(_selectedFile))
                {
                    _selectedFile = "";
                    Console.WriteLine("Start");
                    Console.WriteLine("Audio File is : {0}", audioStream);
                    processPollingLocation = await _serviceClient.IdentifyAsync(audioStream, selectedIds, true);
                    Console.WriteLine("ProcesPolling Location : {0}", processPollingLocation);
                    Console.WriteLine("Done");
                }

                IdentificationOperation identificationResponse = null;
                int numOfRetries = 10;
                TimeSpan timeBetweenRetries = TimeSpan.FromSeconds(5.0);
                while (numOfRetries > 0)
                {
                    await Task.Delay(timeBetweenRetries);
                    identificationResponse = await _serviceClient.CheckIdentificationStatusAsync(processPollingLocation);
                    Console.WriteLine("Response is : {0}", identificationResponse);

                    if (identificationResponse.Status == Microsoft.ProjectOxford.SpeakerRecognition.Contract.Identification.Status.Succeeded)
                    {
                        break;
                    }
                    else if (identificationResponse.Status == Microsoft.ProjectOxford.SpeakerRecognition.Contract.Identification.Status.Failed)
                    {
                        Console.WriteLine("In");
                        speechSynthesizer.SpeakAsync("Failed. Please make sure your voice is registered.");
                        throw new IdentificationException(identificationResponse.Message);
                    }
                    numOfRetries--;
                }
                if (numOfRetries <= 0)
                {
                    voiceIdentifyBtn.IsEnabled = true;
                    throw new IdentificationException("Identification operation timeout.");
                }

                Title = String.Format("Identification Done.");

                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = "Select AccountNo, CustomerName From AccountDetails where AccountNo = (Select AccountNo From AuthenticationDetails where VoiceId = '" + identificationResponse.ProcessingResult.IdentifiedProfileId.ToString() + "')";
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        accountNo = dr.GetInt32(0);
                        voiceIdentifiedUserName = dr[1].ToString();
                        Console.WriteLine("Account No is : " + accountNo);
                        Console.WriteLine("Identified as :" + voiceIdentifiedUserName);
                        _identificationResultTxtBlk.Text = voiceIdentifiedUserName;
                    }
                }
                dr.Close();
                conn.Close();
                if (_identificationResultTxtBlk.Text == "")
                {
                    _identificationResultTxtBlk.Text = identificationResponse.ProcessingResult.IdentifiedProfileId.ToString();
                    speechSynthesizer.SpeakAsync("Sorry we have not found your data.");
                    return;
                }
                else
                {
                    speechSynthesizer.SpeakAsync("Hi.");
                    speechSynthesizer.SpeakAsync(_identificationResultTxtBlk.Text.ToString());
                    speechSynthesizer.SpeakAsync("Thanks to verify your face and voice.");
                    speechSynthesizer.SpeakAsync("Now you can do your transactions");    
                    StartTransactions();
                }

                //if(faceIdentifiedUserName == voiceIdentifiedUserName)
                //{
                //    speechSynthesizer.SpeakAsync("Hi.");
                //    speechSynthesizer.SpeakAsync(_identificationResultTxtBlk.Text.ToString());
                //    speechSynthesizer.SpeakAsync("Thanks to verify your face and voice.");
                //    speechSynthesizer.SpeakAsync("Now you can do your transactions");
                //}
                //else
                //{
                //    speechSynthesizer.SpeakAsync("Sorry we have found different voice identity from your face identity.");
                //    //return;
                //}
                _identificationConfidenceTxtBlk.Text = identificationResponse.ProcessingResult.Confidence.ToString();
                _identificationResultStckPnl.Visibility = Visibility.Visible;
                GC.Collect();
            }
            catch (IdentificationException ex)
            {
                Console.WriteLine("Speaker Identification Error : " + ex.Message);
                GC.Collect();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error : " + ex.Message);
                GC.Collect();
            }
        }

        private void StartTransactions()
        {
            try
            {
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.Text;
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

                speechSynthesizer.SpeakAsync("Hi, Welcome to the NCR.");
                speechSynthesizer.SpeakAsync("Say Hi to know the transaction process details.");
                speechSynthesizer.SpeakAsync("Or Say cancel to cancel transaction.");
                speechRecEngine = new SpeechRecognitionEngine();
                Choices chList1 = new Choices();
                chList1.Add(new string[] { "Hi", "cancel" });
                Grammar grammer1 = new Grammar(new GrammarBuilder(chList1));
                speechRecEngine.RequestRecognizerUpdate();
                speechRecEngine.LoadGrammar(grammer1);
                speechRecEngine.SpeechRecognized += SpeechRecEngine_SpeechRecognized;
                speechRecEngine.SetInputToDefaultAudioDevice();
                speechRecEngine.RecognizeAsync(RecognizeMode.Multiple);
                GC.Collect();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error : " + ex.Message);
                GC.Collect();
            }

        }

        private void SpeechRecEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            try
            {
                Task.Delay(2000);
                switch (e.Result.Text.ToString())
                {
                    case "Hi":
                        
                        speechRecEngine.RecognizeAsyncStop();
                        transaction.DoTransaction(accountNo);
                        break;

                    case "Cancel":
                        speechRecEngine.RecognizeAsyncStop();
                        speechSynthesizer.SpeakAsync("Transaction canceled.");
                        Task.Delay(3000);
                        Environment.Exit(0);
                        break;

                }
                GC.Collect();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error : ", ex.Message);
                GC.Collect();
            }
        }

        
        private void CheckBalance()
        {
            try

            {

                SqlCommand cmd5 = conn.CreateCommand();
                cmd5.CommandType = System.Data.CommandType.Text;
                cmd5.CommandText = "Select AccountBalance From AccountDetails where AccountNo = '" + accountNo + "'";
                dr = cmd5.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        accountBal = dr.GetInt32(0);
                    }
                }
                cmd5.ExecuteNonQuery();
                dr.Close();
                conn.Close();
                speechSynthesizer.SpeakAsync("Hello.");
                speechSynthesizer.SpeakAsync(voiceIdentifiedUserName.ToString());
                speechSynthesizer.SpeakAsync("You have ");
                //accountBal = withdraw.AccountBalAfterWithdraw();
                speechSynthesizer.SpeakAsync(accountBal.ToString());
                speechSynthesizer.SpeakAsync("rupees balance in your account.");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error : " + ex.Message);
            }
        }
    }
}
