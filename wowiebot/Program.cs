﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization;
using System.Timers;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization.Json;
using System.Net.Http;
using System.Windows.Forms;
using wowiebot;

namespace wowiebot
{
    class chatrig
    {
        
        private static byte[] data;
        private static string channel;
        private static NetworkStream stream;

        private static List<string> quotes;
        private static Random rnd = new Random();
        private static int lastQuote = -1;
        private static int lastChoice = -1;
        private static bool addingQuote = false;
        private static List<string> quoteAdders = new List<string>();
        private static string quoteToAdd;
        private static System.Timers.Timer quoteTimer = new System.Timers.Timer(60000);
        private static List<string> eightBallChoices = new List<string>();
        private static List<string> validCommands = new List<string>();
        private static List<bool> displayCommandsInHelp = new List<bool>();

        private static int longestYeahBoiEver;
        private static bool willDisconnect = false;


        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        public static void disconnect()
        {
            willDisconnect = true;
        }

        private static void populateValidCommands(MainForm mainForm, string nick)
        {
            var cfg = Properties.Settings.Default;
            validCommands.Add("help");
            displayCommandsInHelp.Add(false);
            validCommands.Add("commands");
            displayCommandsInHelp.Add(false);
            if (cfg.enableQuotes)
            {
                validCommands.Add("quote");
                displayCommandsInHelp.Add(true);
                validCommands.Add("addquote");
                displayCommandsInHelp.Add(true);
                validCommands.Add("yes");
                displayCommandsInHelp.Add(false);
            }
            if (cfg.enableTitle)
            {
                validCommands.Add("title");
                displayCommandsInHelp.Add(true);
                validCommands.Add("game");
                displayCommandsInHelp.Add(false);
            }
            if (cfg.enableUptime)
            {
                validCommands.Add("uptime");
                displayCommandsInHelp.Add(true);
            }
            if (cfg.enableDiscord)
            {
                validCommands.Add("discord");
                displayCommandsInHelp.Add(true);
            }
            if (nick == "wowiebot")
            {
                validCommands.Add("wowie");
                displayCommandsInHelp.Add(false);
            }
            if (channel == "scatterclegge")
            {
                validCommands.Add("wr");
                displayCommandsInHelp.Add(true);
                validCommands.Add("no");
                displayCommandsInHelp.Add(false);
                validCommands.Add("heck");
                displayCommandsInHelp.Add(false);
            }
            if (channel.ToLower() == "lumardy")
            {
                validCommands.Add("wr");
                displayCommandsInHelp.Add(true);
            }
            if (cfg.enable8Ball)
            {
                validCommands.Add("8ball");
                displayCommandsInHelp.Add(true);
                eightBallChoices.Add("yes");
                eightBallChoices.Add("no");
                eightBallChoices.Add("try again later");
                eightBallChoices.Add("maybe~");
                eightBallChoices.Add("idk ask scatter");
                eightBallChoices.Add("hecc no");
                eightBallChoices.Add("hecc yeah");
                eightBallChoices.Add("you wish");
                eightBallChoices.Add("signs point to yes");
                eightBallChoices.Add("signs point to no");
                eightBallChoices.Add("4 shur");
                eightBallChoices.Add("i know nothing don't ask me again please i'm just a young bot D:");
                eightBallChoices.Add("what do you think ;)");
                eightBallChoices.Add("yank train");
                eightBallChoices.Add("nuns on ripple");
            }
        }

        public static int runBot(MainForm mainForm, string pChannel, string nick, string oauth)
        {
            try
            {
                quoteTimer.Elapsed += QuoteTimer_Elapsed;
                longestYeahBoiEver = Properties.Settings.Default.longestYeahBoiEver;

                //string[] arrQuotes = File.ReadAllLines("quotes.txt");
                //twitchat.Properties.Settings.Default.quotes = new System.Collections.Specialized.StringCollection();
                //twitchat.Properties.Settings.Default.quotes.AddRange(arrQuotes);
                //twitchat.Properties.Settings.Default.Save();
                //quotes = new List<string>(arrQuotes);
                channel = pChannel;
                populateValidCommands(mainForm, nick);
                
                if (Properties.Settings.Default.quotes == null)
                {
                    Properties.Settings.Default.quotes = new System.Collections.Specialized.StringCollection();
                }
                string[] arrQuotes = new string[Properties.Settings.Default.quotes.Count];
                Properties.Settings.Default.quotes.CopyTo(arrQuotes, 0);
                quotes = new List<string>(arrQuotes);
            }
            
            catch (Exception e)
            {
                mainForm.writeToServerOutputTextBox("Error in quote setup. Continuing...\r\n\r\n");
                MessageBox.Show(e.Message);
            }
            TcpClient client;
            try
            {
                int port = 6667;
                client = new TcpClient("irc.twitch.tv", port);
                // Enter in channel (the username of the stream chat you wish to connect to) without the #

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                stream = client.GetStream();
            }
            catch (Exception e)
            {
                mainForm.writeToServerOutputTextBox("Connection error.\r\n\r\n");
                MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }
            

            // Send the message to the connected TcpServer. 

            // string oauth = "5bdocznijbholgvt3o9u6t5ui6okjs";

            string loginstring = "PASS oauth:" + oauth + "\r\nNICK "+ nick +"\r\n";
            Byte[] login = System.Text.Encoding.ASCII.GetBytes(loginstring);
            stream.Write(login, 0, login.Length);
            Console.WriteLine("Sent login.\r\n");
            mainForm.writeToServerOutputTextBox("Sent login.\r\n");
            
            Console.WriteLine(loginstring);
            mainForm.writeToServerOutputTextBox(loginstring);

            // Receive the TcpServer.response.
            // Buffer to store the response bytes.
            data = new Byte[512];

            // String to store the response ASCII representation.
            String responseData = String.Empty;

            // Read the first batch of the TcpServer response bytes.
            Int32 bytes = stream.Read(data, 0, data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            Console.WriteLine("Received WELCOME: \r\n\r\n{0}", responseData);
            mainForm.writeToServerOutputTextBox("Received WELCOME: \r\n\r\n" + responseData);

            // send message to join channel

            string joinstring = "JOIN " + "#" + channel + "\r\n";
            Byte[] join = System.Text.Encoding.ASCII.GetBytes(joinstring);
            stream.Write(join, 0, join.Length);
            Console.WriteLine("Sent channel join.\r\n");
            mainForm.writeToServerOutputTextBox("Sent channel join.\r\n");
            Console.WriteLine(joinstring);
            mainForm.writeToServerOutputTextBox(joinstring);

            // PMs the channel to announce that it's joined and listening
            // These three lines are the example for how to send something to the channel

            string announcestring = channel + "!" + channel + "@" + channel +".tmi.twitch.tv PRIVMSG " + channel + " BOT ENABLED\r\n";
            Byte[] announce = Encoding.ASCII.GetBytes(announcestring);
            stream.Write(announce, 0, announce.Length);
            
            // Lets you know its working
            
            Console.WriteLine("TWITCH CHAT HAS BEGUN.\r\n\r\n");
            mainForm.writeToServerOutputTextBox("TWITCH CHAT HAS BEGUN.");
            Console.WriteLine("\r\nBE CAREFUL.\r\n");
            mainForm.writeToServerOutputTextBox("\r\nBE CAREFUL.\r\n");

            string helpCommands = "Use me in the following ways: ";
            for (int i = 0; i < validCommands.Count; i++)
            {
                if (displayCommandsInHelp[i])
                {
                    helpCommands += Properties.Settings.Default.prefix + validCommands[i] + ", ";
                }
            }
            helpCommands = helpCommands.Substring(0, helpCommands.Length - 2);

            willDisconnect = false;

            while (!willDisconnect)
            {
                
                // build a buffer to read the incoming TCP stream to, convert to a string

                byte[] myReadBuffer = new byte[1024];
                StringBuilder myCompleteMessage = new StringBuilder();
                int numberOfBytesRead = 0;

                // Incoming message may be larger than the buffer size.
                do
                {
                    try { numberOfBytesRead = stream.Read(myReadBuffer, 0, myReadBuffer.Length); }
                    catch (Exception e)
                    {
                        Console.WriteLine("OH SHIT SOMETHING WENT WRONG\r\n", e);
                        mainForm.writeToServerOutputTextBox("OH SHIT SOMETHING WENT WRONG\r\n");
                    }

                    myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));
                }
                
                // when we've received data, do Things
                
                while (stream.DataAvailable);

                // Print out the received message to the console.
                Console.WriteLine(myCompleteMessage.ToString());
                mainForm.writeToServerOutputTextBox(myCompleteMessage.ToString());
                switch (myCompleteMessage.ToString())
                {
                    // Every 5 minutes the Twitch server will send a PING, this is to respond with a PONG to keepalive
                    
                    case "PING :tmi.twitch.tv\r\n":
                        try { 
                        Byte[] say = System.Text.Encoding.ASCII.GetBytes("PONG :tmi.twitch.tv\r\n");
                        stream.Write(say, 0, say.Length);
                        Console.WriteLine("Ping? Pong!");
                        mainForm.writeToServerOutputTextBox("Ping? Pong!");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("OH SHIT SOMETHING WENT WRONG\r\n", e);
                            mainForm.writeToServerOutputTextBox("OH SHIT SOMETHING WENT WRONG\r\n");
                        }
                        break;
                        
                    // If it's not a ping, it's probably something we care about.  Try to parse it for a message.
                    default:
                        try
                        {
                            string messageParser = myCompleteMessage.ToString();
                            char[] spl = {':'};
                            string[] message = messageParser.Split(spl, 3);
                            string[] preamble = message[1].Split(' ');
                            string tochat;

                            // This means it's a message to the channel.  Yes, PRIVMSG is IRC for messaging a channel too
                            if (preamble[1] == "PRIVMSG")
                            {
                                string[] sendingUser = preamble[0].Split('!');
                                tochat = sendingUser[0] + ": " + message[2];

                                // sometimes the carriage returns get lost (??)
                                if (tochat.Contains("\n") == false)
                                {
                                    tochat = tochat + "\n";
                                }

                                // Ignore some well known bots
                                if (sendingUser[0] != "moobot" && sendingUser[0] != "whale_bot")
                                {
                                    //  SendKeys.SendWait(tochat.TrimEnd('\n'));
                                }




                                if (message[2].StartsWith(Properties.Settings.Default.prefix))
                                {
                                    string command;
                                    if (message[2].Contains(" "))
                                        command = message[2].Substring(1, message[2].IndexOf(" ") - 1);
                                    else
                                        command = message[2].Substring(1, message[2].Length - 3);

                                    command = command.ToLower();

                                    if (validCommands.Contains(command))
                                    {

                                        switch (command)
                                        {
                                            case "help":
                                            case "commands":
                                                
                                                sendMessage(helpCommands);
                                                break;

                                            case "title":
                                            case "game":
                                                {
                                                    HttpWebRequest apiRequest = (HttpWebRequest)WebRequest.Create("https://api.twitch.tv/kraken/channels/" + channel);
                                                    apiRequest.Accept = "application/vnd.twitchtv.v2+json";
                                                    apiRequest.Headers.Add("Client-ID: jqqcl6f383moz9gzdd3aeg7lt4h0t0");


                                                    Stream apiStream;

                                                    apiStream = apiRequest.GetResponse().GetResponseStream();
                                                    StreamReader apiReader = new StreamReader(apiStream);
                                                    string jsonData = apiReader.ReadToEnd();
                                                    
                                                    JObject parsed = JObject.Parse(jsonData);

                                                    apiReader.Close();
                                                    apiReader.Dispose();

                                                    apiStream.Close();
                                                    apiStream.Dispose();



                                                    sendMessage(channel + " is streaming " + parsed.Property("game").Value.ToString() + ": \"" + parsed.Property("status").Value.ToString() + "\"");
                                                }
                                                break;

                                            case "discord":
                                                sendMessage("Join my Discord server! You can only be cool if you do this first. " + wowiebot.Properties.Settings.Default.discordServer);
                                                break;

                                            case "uptime":
                                                {
                                                    HttpWebRequest apiRequest = (HttpWebRequest)WebRequest.Create("https://api.twitch.tv/kraken/streams/" + channel);
                                                    apiRequest.Accept = "application/vnd.twitchtv.v2+json";
                                                    apiRequest.Headers.Add("Client-ID: jqqcl6f383moz9gzdd3aeg7lt4h0t0");


                                                    Stream apiStream;

                                                    apiStream = apiRequest.GetResponse().GetResponseStream();
                                                    StreamReader apiReader = new StreamReader(apiStream);
                                                    string jsonData = apiReader.ReadToEnd();
                                                    JObject parsed = JObject.Parse(jsonData);
                                                    JObject streamParsed = JObject.Parse(parsed.Property("stream").Value.ToString());

                                                    apiReader.Close();
                                                    apiReader.Dispose();

                                                    apiStream.Close();
                                                    apiStream.Dispose();

                                                    if (parsed.Property("stream").Value.ToString() == "")
                                                    {
                                                        sendMessage("Stream is not live. How are you seeing this?");
                                                        break;
                                                    }
                                                    string time = streamParsed.Property("created_at").ToString();
                                                    DateTime liveTime = DateTime.Parse(streamParsed.Property("created_at").Value.ToString());
                                                    //   DateTime liveTime = DateTime.ParseExact(streamParsed.Property("created_at").ToString(), "", new System.Globalization.CultureInfo("en-US"), System.Globalization.DateTimeStyles.None); 
                                                    TimeSpan uptime = DateTime.Now.ToUniversalTime() - liveTime;

                                                    sendMessage(channel + " has been live for " + uptime.Hours + " hours and " + uptime.Minutes + " minutes.");

                                                }

                                                break;

                                            case "quote":
                                                if (quotes.Count == 0)
                                                {
                                                    sendMessage("No quotes. I guess " + channel + " just isn't funny or quotable.");
                                                    break;
                                                }
                                                int q;
                                                do
                                                {
                                                    q = rnd.Next(quotes.Count);
                                                } while (q == lastQuote && quotes.Count > 1);
                                                sendMessage(quotes[q]);
                                                lastQuote = q;
                                                break;

                                            case "addquote":

                                                if (addingQuote)
                                                {
                                                    sendMessage("Finish adding the current quote first.");
                                                    break;
                                                }


                                                quoteToAdd = message[2].Substring(message[2].IndexOf(" ") + 1);
                                                quoteToAdd = quoteToAdd.Remove(quoteToAdd.Length - 2);
                                                addingQuote = true;
                                                quoteTimer.Start();

                                                sendMessage("Two other people need to agree by typing !yes to add the quote! Ends in one minute.");

                                                quoteAdders.Add(sendingUser[0]);

                                                break;

                                            case "yes":
                                                if (!addingQuote)
                                                    break;

                                                if (!quoteAdders.Contains(sendingUser[0]))
                                                {
                                                    quoteAdders.Add(sendingUser[0]);
                                                    if (quoteAdders.Count == 2)
                                                    {
                                                        sendMessage("One more!");
                                                    }
                                                    else if (quoteAdders.Count == 3)
                                                    {
                                                        wowiebot.Properties.Settings.Default.quotes.Add(quoteToAdd);
                                                        wowiebot.Properties.Settings.Default.Save();
                                                        quoteAdders.Clear();
                                                        quotes.Add(quoteToAdd);
                                                        addingQuote = false;
                                                        quoteTimer.Stop();
                                                        sendMessage("Quote added.");
                                                    }
                                                }

                                                else if (quoteAdders[0] == sendingUser[0])
                                                {
                                                    sendMessage("Yeah, you added the quote. I got it.");
                                                }

                                                else
                                                {
                                                    sendMessage("You already voted, dingus");
                                                }

                                                break;

                                            case "no":
                                                if (!addingQuote)
                                                    break;
                                                sendMessage("Smartass.");
                                                break;

                                            case "wowie":
                                                sendMessage("wowie");
                                                break;

                                            case "wr":
                                                sendMessage("https://www.youtube.com/watch?v=okR63Hh6ONU");
                                                break;

                                            case "heck":
                                                sendMessage("https://twitter.com/billyraycyrus/status/335910871974965248");
                                                break;
                                            case "8ball":
                                                int p;
                                                do
                                                {
                                                    p = rnd.Next(eightBallChoices.Count);
                                                } while (p == lastChoice && eightBallChoices.Count > 1);
                                                sendMessage(eightBallChoices[p]);
                                                lastChoice = p;
                                                break;

                                            default:
                                                break;
                                        }
                                    }
                                }

                                else if (Properties.Settings.Default.enableYeahBoi
                                && message[2].ToLower().Contains("wowie")
                                && message[2].ToLower().Contains("longest")
                                && message[2].ToLower().Contains("ever")
                                && (message[2].ToLower().Contains("yeah boi") || message[2].ToLower().Contains("yeah boy")))
                                {
                                    sendMessage("yeah bo" + new string('i', longestYeahBoiEver++));
                                    Properties.Settings.Default.longestYeahBoiEver++;
                                    Properties.Settings.Default.Save();
                                }

                                if (Properties.Settings.Default.enableLinkTitles)
                                {
                                    Regex regx = new Regex(@"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)", RegexOptions.IgnoreCase);
                                    MatchCollection mactches = regx.Matches(message[2]);
                                    foreach (Match match in mactches)
                                    {
                                        WebClient x = new WebClient();
                                        string source;
                                        //WebRequest req = WebRequest.Create(match.Value);
                                        try
                                        {
                                            source = x.DownloadString(match.Value);
                                        }
                                        catch
                                        {
                                            continue;
                                        }
                                        string title = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
                                        title = Regex.Replace(title, @"[^\u0000-\u007F]+", string.Empty);
                                        if (title != null && title != "")
                                        {
                                            sendMessage(sendingUser[0] + " posted: " + title);
                                        }
                                    }
                                }
                                
                            }
                            // A user joined.
                            else if (preamble[1] == "JOIN")
                            {
                                string[] sendingUser = preamble[0].Split('!');
                                tochat = "JOINED: " + sendingUser[0];
                            //    Console.WriteLine(tochat);
                               // SendKeys.SendWait(tochat.TrimEnd('\n'));
                            }
                        }
                        // This is a disgusting catch for something going wrong that keeps it all running.  I'm sorry.
                        catch (Exception e)
                        {
                            Console.WriteLine("OH SHIT SOMETHING WENT WRONG\r\n", e);
                            mainForm.writeToServerOutputTextBox("OH SHIT SOMETHING WENT WRONG\r\n");
                        }

                        // Uncomment the following for raw message output for debugging
                        //
                        // Console.WriteLine("Raw output: " + message[0] + "::" + message[1] + "::" + message[2]);
                        // Console.WriteLine("You received the following message : " + myCompleteMessage);
                        break;
                }
            }

            stream.Close();
            client.Close();
            return 0;
        }

        private static void QuoteTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            sendMessage("Time's up. I guess no one thought that quote was funny.");
            quoteAdders.Clear();
            addingQuote = false;
            quoteTimer.Stop();
        }

        private static void sendMessage(string message)
        {
            Byte[] say = Encoding.ASCII.GetBytes("PRIVMSG #" + channel + " :" + message + "\r\n");
            stream.Write(say, 0, say.Length);
        }
    }
}
