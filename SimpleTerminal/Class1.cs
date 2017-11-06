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
        public bool isReady = false;
        #endregion

        #region IterminalInterface Methods
        public override TransactionResult Debit(TransactionData transactionData)
        {
            throw new NotImplementedException();
        }

        public override TransactionResult Debit(TransactionData transactionData, Card card)
        {
            TransactionFailedResult txFailed = new TransactionFailedResult(TransactionType.Debit, DateTime.Now);
            // perform the credit operation
            // ...

            requestMessage = new RequestMessage();
            requestMessage.createCompleteMessage(requestMessage);
            serialPort.SendMessage(requestMessage.finalMessage);
            //The terminal is really slow  it takes about 17 seconds to read the entire response.
            Thread.Sleep(20000);
            //responseMessage =
            string trimmed = response.Replace("-", String.Empty);
            this.OnTrace(TraceLevel.Info, "the message " + trimmed);



            return txFailed;


            // create the result data
            //if (creditOk)
            //{
            //    // handling for transaction was successful (set amount and timestamp)
            //    TransactionDoneResult doneResult = new TransactionDoneResult(TransactionType.Credit, DateTime.Now);
            //    doneResult.ServiceCode = "SC4711";
            //    doneResult.TransactionNumber = "AZ12345678";
            //    return doneResult;
            //}
            //else
            //{
            //    // handling for transaction failed
            //    TransactionFailedResult failedResult = new TransactionFailedResult(TransactionType.Credit);
            //    failedResult.OperatorDisplayText = "Can't read card, try again!";
            //    failedResult.RejectionCode = 1;    // "Can't read card"
            //    return failedResult;
            //}
        }
        string response = null;
        private void SerialPort_MessageReceived(object sender, MessageReceivedEventArgs args)
        {

            response += BitConverter.ToString(args.Data);
            //response.Trim(new char[] { '-', ' ' });

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
            cards.Add(new CardIssuer("MAEST"));
            // Se deve consultar las abreviaciones para mastercard. segun el manual.
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
    }
}
