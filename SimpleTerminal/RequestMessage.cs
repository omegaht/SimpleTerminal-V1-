using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace SimpleTerminal
{
    class RequestMessage
    {
        private const string STX = "02";
        private const string ETX = "03";
        public string LRC { get; set; }
        /// <summary>
        /// C51 for request, C54 for response.
        /// </summary>
        private string messageType = "433531";
        private string lengthTLV;

        /// <summary>
        /// The entire chain, from STX to ETX.
        /// </summary>
        public string message { get; }
        /// <summary>
        /// The message in byte representation including the LRC, this is the one sent to the Terminal.
        /// </summary>
        public byte[] finalMessage;

        #region TLV Members
        public string transactionDate;
        public string transactionTime;
        private string transactionType;
        private string voucherNumber;
        public string amount { get; }
        private string cashBack = "C10400000000";
        private string tipAmount = "C10400000000";
        private string amountUSD;
        private string cashBackUSD = "C10400000000";
        private string tipAmountUSD = "C10400000000";
        private string currencyIndex;
        private string numberPump = "C10100";
        private string authorizationCode;
        private string merchantDecision;
        private string ECRIDPOS;
        //private string emvTags { get; set; }
        private string E1 { get; set; }
        #endregion

        /// <summary>
        /// Constructor with fixed parameters.
        /// </summary>
        public RequestMessage(string amount = "C104000015B3")
        {
            #region Load TLV parameters
            DateTime now = DateTime.Now;
            transactionDate = FormatDate(now);
            transactionTime = FormatTime(now);
            //transactionDate = "C103101231";
            //transactionTime = "C103164459";
            transactionType = "C10101";
            voucherNumber = "C10100";
            this.amount = amount;
            cashBack = "C10400000000";
            tipAmount = "C10400000000";
            amountUSD = "C104000022B8";
            cashBackUSD = "C10400000000";
            tipAmountUSD = "C10400000000";
            
            currencyIndex = "C10101";
            //numberPump = ""
            authorizationCode = "C10100";
            merchantDecision = "C10100";
            ECRIDPOS = "C1143030303030303030303130333031353131393030";
            E1 = "E1255F2A8284959A9C9F029F039F099F109F1A9F1E9F269F279F339F349F359F369F379F419F53";
            //Lenght of the TLV parameters.
            string length =
                ((transactionDate.Length +
                    transactionTime.Length +
                    transactionType.Length +
                    voucherNumber.Length +
                    amount.Length +
                    cashBack.Length +
                    tipAmount.Length +
                    amountUSD.Length +
                    cashBackUSD.Length +
                    tipAmountUSD.Length +
                    currencyIndex.Length +
                    numberPump.Length +
                    authorizationCode.Length +
                    merchantDecision.Length +
                    ECRIDPOS.Length +
                    E1.Length) / 2).ToString();
            string hexLength = Convert.ToString(int.Parse(length), 16);
            lengthTLV = hexLength;
            #endregion
            #region Join Message fields
            message =
                    STX +
                    messageType +
                    "00" +
                    lengthTLV +
                    transactionDate +
                    transactionTime +
                    transactionType +
                    voucherNumber +
                    amount +
                    cashBack +
                    tipAmount +
                    amountUSD +
                    cashBackUSD +
                    tipAmountUSD +
                    currencyIndex +
                    numberPump +
                    authorizationCode +
                    merchantDecision +
                    ECRIDPOS +
                    E1 +
                    ETX;
            #endregion
        }

        public void createCompleteMessage(RequestMessage m)
        {
            // no STX for LRC calculation...
            byte[] truncatedMessage = StringToByteArray(m.message.Substring(2));
            byte LRC = calculateLRC(truncatedMessage);
            //LRCA = calculateLRC(truncatedMessage);
            byte[] tempMessage = StringToByteArray(m.message);
            //convert it to string hexadecimal representation and append it to the final message.
            m.LRC = Convert.ToString(int.Parse(LRC.ToString()), 16);
            byte[] finalLRC = StringToByteArray(this.LRC);
            // Plus one, in order to insert the LRC.
            m.finalMessage = new byte[tempMessage.Length + 1];
            // copy the arry and insert the LRC
            for (int i = 0; i < tempMessage.Length; i++)
                m.finalMessage[i] = tempMessage[i];
            m.finalMessage[tempMessage.Length] = finalLRC[0];
        }

        #region Message conversion methods
        public static byte calculateLRC(byte[] bytes)
        {
            byte LRC = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                LRC ^= bytes[i];
            }
            return LRC;
        }
        /// <summary>
        /// Converts the exact string to a byte array.
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars - 1; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
        #endregion

        #region String Manipulation methods
        
        public string FormatDate(DateTime now)
        {
            CultureInfo culture = new CultureInfo("ja-JP");
            string theDate = now.ToString("d", culture);
            string[] splitedDate = theDate.Split('/');
            string formatedDate = splitedDate[0].Substring(2,2) + splitedDate[1] + splitedDate[2];
            
            
            return "C103" + formatedDate;
            

        }
        public string FormatTime(DateTime now)
        {
            string hour = "";
            string time = now.GetDateTimeFormats('T')[0];
            string[] splitedTime = time.Split(' ');
            string[] splitedTimeAgain = splitedTime[0].Split(':');

            if (splitedTimeAgain[0].Length == 1)
                hour = "0" + splitedTimeAgain[0];
            else
                hour = splitedTimeAgain[0];

            string formatedTime = hour + splitedTimeAgain[1] + splitedTimeAgain[2];
            return "C103" + formatedTime;
        }
        #endregion
    }
}

