using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SkiData.ElectronicPayment;
using SkiData.ElectronicPayment.BasicTerminal;
using Chapi.SerialPortLib.Communication;



namespace SimpleTerminal
{
    public class Class1 : BasicTerminal
    {
        #region Public Members
        public SerialPortCom serialPort = new SerialPortCom();
        RequestMessage requestMessage;
        ResponseMessage responseMessage;
        public bool isReady = false;
        string response = null;
        #endregion

        #region IterminalInterface Methods
        public override bool IsTerminalReady()
        {
            //Send the Enquiry character 0x05.
            byte[] ENQ = { 0x05,0x03 };
            byte LRC = calculateLRC(ENQ);
            byte[] message = { 0x02, 0x05, 0x03, LRC };
            serialPort.SendMessage(message);
            Thread.Sleep(2000);
            //If ACK, ther terminal is ready.
            if(response == "06")
                return true;
            return false;
        }

        public override TransactionResult Debit(TransactionData transactionData)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Performs the Debit operation.
        /// </summary>
        /// <param name="transactionData"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        public override TransactionResult Debit(TransactionData transactionData, Card card)
        {
            #region Perform debit operation
            // clear the response.
            response = null;
            // create request message.
            requestMessage = new RequestMessage();
            requestMessage.createCompleteMessage(requestMessage);
            // send message to the terminal.
            serialPort.SendMessage(requestMessage.finalMessage);
            // wait for ther terminal to reply.
            Thread.Sleep(20000);
            // create the response message.
            string trimmed = cleanMessage(response);
            responseMessage = new ResponseMessage(trimmed);
            // verify if LRC is valid.
            bool validLRC = responseMessage.verifyResponseLRC(responseMessage);
            #endregion

            // TESt only
            this.OnTrace(TraceLevel.Info, "host Response :" + responseMessage.terminalResponse);
            this.OnTrace(TraceLevel.Info, "STX:" + responseMessage.STX);
            this.OnTrace(TraceLevel.Info, "messageType :" + responseMessage.messageType);
            this.OnTrace(TraceLevel.Info, "Status: " + responseMessage.messageStatus);
            this.OnTrace(TraceLevel.Info, "TLV length: " + responseMessage.lengthTLV);

            this.OnTrace(TraceLevel.Info, "auth Code : " + responseMessage.authorizationCode);
            this.OnTrace(TraceLevel.Info, "responseCode : " + responseMessage.responseCode);
            this.OnTrace(TraceLevel.Info, "transactionDate: " + responseMessage.transactionDate);
            this.OnTrace(TraceLevel.Info, "transactionTime: " + responseMessage.transactionTime);
            this.OnTrace(TraceLevel.Info, "voucherNumber: " + responseMessage.voucherNumber);
            this.OnTrace(TraceLevel.Info, "cardNumber: " + responseMessage.cardNumber);
            this.OnTrace(TraceLevel.Info, "cardHolderName: " + responseMessage.cardHolderName);
            this.OnTrace(TraceLevel.Info, "cardEntryMode: " + responseMessage.cardEntryMode);

            this.OnTrace(TraceLevel.Info, "cardType: " + responseMessage.cardType);
            this.OnTrace(TraceLevel.Info, "currencyCode: " + responseMessage.currencyCode);
            this.OnTrace(TraceLevel.Info, "amountAuthorized: " + responseMessage.amountAuthorized);
            this.OnTrace(TraceLevel.Info, "softwareVersion: " + responseMessage.softwareVersion);
            this.OnTrace(TraceLevel.Info, "serialNumber :" + responseMessage.serialNumber);
            this.OnTrace(TraceLevel.Info, "E1 :" + responseMessage.E1);
            this.OnTrace(TraceLevel.Info, "E2 :" + responseMessage.E2);
            this.OnTrace(TraceLevel.Info, "ETX :" + responseMessage.ETX);
            this.OnTrace(TraceLevel.Info, "LRC :" + responseMessage.LRC);
            this.OnTrace(TraceLevel.Info, "EOT :" + responseMessage.EOT);

            if (!validLRC)
            {
                TransactionFailedResult txFailed = new TransactionFailedResult(TransactionType.Debit, DateTime.Now);
                return txFailed;
            }
            else
            {
                TransactionDoneResult txSucceed = new TransactionDoneResult(TransactionType.Debit, DateTime.Now);
                return txSucceed;
            }


        }

        public override bool BeginInstall(TerminalConfiguration termConfig)
        {
            this.OnTrace(TraceLevel.Info, "BasicTerminal.BeginInstall");
            #region Port communication 
            // Configure parameters of the serial port.

            serialPort.SetPort(termConfig.CommunicationChannel);
            // Establish connection with the port.
            if (serialPort.Connect())
            {
                this.OnTrace(TraceLevel.Info, "Connection established with device");
                #region Terminal configuration
                // Trace Message
                this.OnTrace(TraceLevel.Info,
                    "BasicTerminal.BeginInstall - TerminalConfiguration\n" +
                    "CommChannel  = {0}\n" +
                    "Currency     = {1}\n" +
                    "CustDisp.CPL = {2}\n" +
                    "CustDisp.NOL = {3}\n" +
                    "DeviceID     = {4}\n" +
                    "DeviceName   = {5}\n" +
                    "DeviceType   = {6}\n" +
                    "OperDisp.CPL = {7}\n" +
                    "OperDisp.NOL = {8}\n" +
                    "Receipt.CPL  = {9}\n" +
                    "TerminalID   = '{10}'\n",
                    termConfig.CommunicationChannel,
                    termConfig.Currency,
                    termConfig.CustomerDisplay.CharactersPerLine,
                    termConfig.CustomerDisplay.NumberOfLines,
                    termConfig.DeviceId,
                    termConfig.DeviceName,
                    termConfig.DeviceType.ToString(),
                    termConfig.OperatorDisplay.CharactersPerLine,
                    termConfig.OperatorDisplay.NumberOfLines,
                    termConfig.ReceiptConfiguration.CharactersPerLine,
                    termConfig.TerminalId);
                #endregion
                //subscribe to reader event
                serialPort.MessageReceived += SerialPort_MessageReceived;
                return true;
            }
            else
            {
                this.OnTrace(TraceLevel.Error, "Error - Couldn´t connect with device");
                return false;
            }
            #endregion

        }

        public override void AllowCards(CardIssuerCollection cards)
        {
            // update the internal list with the card issuers
            cards.Clear();
            cards.Add(new CardIssuer("VISA"));
            cards.Add(new CardIssuer("MC"));
            cards.Add(new CardIssuer("MAEST"));
            // Se debe consultar las abreviaciones para mastercard. segun el manual.
            this.OnTrace(TraceLevel.Info, "Allowed Cards");
            foreach (CardIssuer item in cards)
            {
                this.OnTrace(TraceLevel.Info, item.Abbreviation);
            }

        }

        public override void EndInstall()
        {
            isReady = this.IsTerminalReady();
            this.OnTrace(TraceLevel.Info, "BasicTerminal.EndInstall");
            this.OnTrace(TraceLevel.Info, "The terminal is ready for use");
        }

        public override void Dispose()
        {
            this.OnTrace(TraceLevel.Info, "BasicTerminal.Dispose");
            serialPort.Disconnect();
            if (serialPort.IsConnected)
                this.OnTrace(TraceLevel.Error, "ComPort not Disconnected");
            else
                this.OnTrace(TraceLevel.Info, "ComPort Disconnected!");
        }
        #endregion

        #region Our Event subscriptions
        private void SerialPort_MessageReceived(object sender, MessageReceivedEventArgs args)
        {
            response += BitConverter.ToString(args.Data);
        }
        #endregion


        public static byte calculateLRC(byte[] bytes)
        {
            byte LRC = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                LRC ^= bytes[i];
            }
            return LRC;
        }
        #region String manipulation methods

        /// <summary>
        /// Replaces the '-' characters from the response of the terminal.
        /// </summary>
        /// <param name="message">The response from the terminal</param>
        /// <returns></returns>
        public string cleanMessage(string message)
        {
            string formatedMessage = message.Replace("-", string.Empty);
            return formatedMessage;
        }
        #endregion

    }
}
