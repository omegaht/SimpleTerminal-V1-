using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleTerminal
{
    class ResponseMessage
    {
        
        public  string ETX ;
        public string ACK { get; }
        public string LRC { get; }
        public string EOT;
        /// <summary>
        /// The entire chain, from STX to ETX.
        /// </summary>
        public string message { get; set; }



        
        public string totalResponse { get; set; }
        public string STX;
        public string messageType;
        public string messageStatus;
        public string lengthTLV;
        #region TLV Members
        public string hostResponse;
        public string authorizationCode;
        public string responseCode;
        public string transactionDate;
        public string transactionTime;
        public string voucherNumber;
        public string cardNumber;
        public string cardHolderName;
        public string cardEntryMode;
        public string numberPump;
        public string cardType;
        public string currencyCode;
        public string amountAuthorized;
        public string softwareVersion;
        public string serialNumber;
        public string E1;
        public string E2;
        #endregion

        public ResponseMessage(string data)
        {

            message = data;
            totalResponse = data.Substring(0, 2);
            STX = data.Substring(2, 2);
            messageType = data.Substring(4, 6);
            messageStatus = data.Substring(10, 4);
            lengthTLV = data.Substring(14, 4);
            hostResponse = data.Substring(18, 6);
            authorizationCode = data.Substring(24, 16);
            responseCode = data.Substring(40, 8);
            transactionDate = data.Substring(48, 10);
            transactionTime = data.Substring(58, 10);
            voucherNumber = data.Substring(68, 12);
            //string tempData = data.Substring(80, data.Length);
            int longitud = data.IndexOf("C1", 82) - 80;
            cardNumber = data.Substring(80, longitud);
            int position = 80 + longitud;
            longitud = data.IndexOf("C1", position + 2) - position;
            cardHolderName = data.Substring(position, longitud);
            position += longitud;
            cardEntryMode = data.Substring(position, 8);
            cardType = data.Substring(position + 8, 6);
            currencyCode = data.Substring(position + 14, 8);
            amountAuthorized = data.Substring(position + 22, 12);
            softwareVersion = data.Substring(position + 34, 44);
            serialNumber = data.Substring(position + 78, 44);
            position += 122;
            longitud = data.IndexOf("E2") - position;
            E1 = data.Substring(position, longitud);
            position += longitud;
            longitud = data.IndexOf("03", position) - position;
            E2 = data.Substring(position, longitud);
            position += longitud;
            longitud = 2;
            ETX = data.Substring(position, longitud);
            position += longitud;
            LRC = data.Substring(position, longitud);
            position += longitud;
            EOT = data.Substring(position, longitud);
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

        public bool verifyResponseLRC(ResponseMessage m)
        {
            m.message = m.message.Substring(4);
            m.message = m.message.Substring(0, m.message.Length - 4);


            byte[] truncatedMessage = StringToByteArray(m.message);

            byte LRC = calculateLRC(truncatedMessage);
            byte[] responseLRC = StringToByteArray(this.LRC);

            if (LRC == responseLRC[0]) { 
                return true;
            }
            return false;
        }


    }


}

