using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleTerminal
{
    class ResponseMessage
    {
        private const string STX = "02";
        private const string ETX = "03";
        public string LRC { get; }
        /// <summary>
        ///  C54 for response.
        /// </summary>
        private string messageType = "433534";
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
        private string hostResponse;
        private string authorizationCode;
        private string responseCode;
        private string transactionDate;
        private string transactionTime;
        private string voucherNumber;
        private string cardNumber;
        private string cardHolderName;
        private string cardEntryMode;
        private string numberPump;
        private string cardType;
        private string currencyCode;
        private string amountAuthorized;
        private string softwareVersion;
        private string serialNumber;
        private string E1;
        private string E2;
        #endregion
    }
}
