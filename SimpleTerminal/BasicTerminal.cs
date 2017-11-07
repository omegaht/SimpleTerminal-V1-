using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using SkiData.ElectronicPayment;

namespace SkiData.ElectronicPayment.BasicTerminal
{
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    public abstract class BasicTerminal : ITerminal
    {
        #region Private Members

        private Settings settings = new Settings();

        #endregion

        #region Construction

        public BasicTerminal()
        {
            // TODO
            this.settings.AllowsCancel = false;
            this.settings.AllowsCredit = false;
            this.settings.AllowsRepeatReceipt = false;
            this.settings.AllowsValidateCard = false;
            this.settings.AllowsVoid = false;
            this.settings.CanSetCardData = true;
            this.settings.HasCardReader = false;
            this.settings.IsContactless = false;
            this.settings.MayPrintReceiptOnRejection = false;
            this.settings.NeedsSkidataChipReader = false;
            this.settings.RequireReceiptPrinter = false;
        }

        #endregion

        #region ITerminal Members

        public virtual void Dispose()
        {
            this.OnTrace(TraceLevel.Info, "BasicTerminal.Dispose");
        }

        /// <summary>
        /// Note to implementers: Use TerminalSettings to access the base functionality
        /// </summary>
        Settings ITerminal.Settings
        {
            get { return this.TerminalSettings; }
        }

        protected Settings TerminalSettings
        {
            get { return this.settings; }
        }

        public string Name
        {
            get { return "SkiData.ElectronicPayment.BasicTerminal"; }
        }

        public string ShortName
        {
            get { return "BasicTerminal"; }
        }

        public virtual bool BeginInstall(TerminalConfiguration termConfig)
        {
            this.OnTrace(TraceLevel.Info, "BasicTerminal.BeginInstall");
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
            return true;
        }

        public virtual void AllowCards(CardIssuerCollection cards)
        {
            foreach (CardIssuer cardIssuer in cards)
            {
                this.OnTrace(TraceLevel.Info,
                    "BasicTerminal.AllowCards - CardIssuer: {0}",
                    cardIssuer.Abbreviation);
            }
        }

        public virtual void EndInstall()
        {
            this.OnTrace(TraceLevel.Info, "BasicTerminal.EndInstall");
        }

        public void Cancel()
        {
        }

        public TransactionResult Credit(TransactionData transactionData)
        {
            TransactionFailedResult txFailed = new TransactionFailedResult(TransactionType.Credit, DateTime.Now);
            txFailed.CustomerDisplayText = "";
            txFailed.OperatorDisplayText = "Not implemented";
            txFailed.ReceiptPrintoutMandatory = false;
            txFailed.Receipts = new Receipts();
            txFailed.RejectionCode = 23;
            return txFailed;
        }

        public TransactionResult Credit(TransactionData transactionData, Card card)
        {
            return this.Credit(transactionData);
        }

        public abstract TransactionResult Debit(TransactionData transactionData);

        public abstract TransactionResult Debit(TransactionData transactionData, Card card);

        public void ExecuteCommand(int commandType)
        {
            // no special action needed, just trace
            this.OnTrace(TraceLevel.Verbose,
                "BasicTerminal.ExecuteCommand - commandType = {0}",
                commandType);
        }

        public void ExecuteCommand(int commandType, object parameter)
        {
            // no special action needed, just trace
            this.OnTrace(TraceLevel.Verbose,
                "BasicTerminal.ExecuteCommand - commandType = {0}, parameter = {1}",
                commandType, parameter.ToString());
        }

        public void Notify(int notificationId)
        {
            // no special action needed, just trace
            this.OnTrace(TraceLevel.Verbose,
                "BasicTerminal.Notify - notificationId = {0}",
                notificationId);
        }

        public CommandDefinitionCollection GetCommands()
        {
            return new CommandDefinitionCollection();
        }

        public TransactionResult GetLastTransaction()
        {
            TransactionResult trRes = new TransactionDoneResult(TransactionType.Debit, DateTime.Now);
            return trRes;
        }

        public Card GetManualCard(Card card)
        {
            return GetManualCard(card, "");
        }

        public Card GetManualCard(Card card, string paymentType)
        {
            return card;
        }

        public Parameter GetParameter(string key)
        {
            return new Parameter(key, "");
        }

        public virtual bool IsTerminalReady()
        {
            return true;
        }
       

        public Card OpenInputDialog(IntPtr windowHandle, TransactionType transactionType, Card card)
        {
            return Card.NoCard;
        }

        public Receipts RepeatReceipt()
        {
            return new Receipts();
        }

        public void SetDisplayLanguage(System.Globalization.CultureInfo cultureInfo)
        {
            this.OnTrace(TraceLevel.Verbose,
                "BasicTerminal.SetDisplayLanguage - CultureInfo: {0:X4}  {1}",
                cultureInfo.LCID, cultureInfo.DisplayName);
        }

        public void SetParameter(Parameter parameter)
        {
            this.OnTrace(TraceLevel.Verbose,
                "BasicTerminal.SetParameter - Parameter received: Key: '{0}', Value: '{1}'",
                parameter.Key, parameter.Value);
        }

        public bool SupportsCreditCards()
        {
            return true;
        }

        public bool SupportsDebitCards()
        {
            return true;
        }

        public bool SupportsElectronicPurseCards()
        {
            return true;
        }

        public ValidationResult ValidateCard()
        {
            ValidationResult valResult = new ValidationResult();
            valResult.CustomerDisplayText = "";
            valResult.OperatorDisplayText = "";
            valResult.RejectionCode = 1;
            return valResult;
        }

        public ValidationResult ValidateCard(Card card)
        {
            return this.ValidateCard();
        }

        public TransactionResult Void(TransactionDoneResult transactionResult)
        {
            TransactionFailedResult txFailed = new TransactionFailedResult(TransactionType.Void, DateTime.Now);
            txFailed.CustomerDisplayText = "";
            txFailed.OperatorDisplayText = "Not implemented";
            txFailed.ReceiptPrintoutMandatory = false;
            txFailed.Receipts = new Receipts();
            txFailed.RejectionCode = 23;
            return txFailed;
        }

        #endregion

        #region ITerminal Events

        public event ActionEventHandler Action;
        public event ChoiceEventHandler Choice;
        public event ConfirmationEventHandler Confirmation;
        public event DeliveryCheckEventHandler DeliveryCheck;
        public event ErrorClearedEventHandler ErrorCleared;
        public event ErrorOccurredEventHandler ErrorOccurred;
        public event IrregularityDetectedEventHandler IrregularityDetected;
        public event EventHandler TerminalReadyChanged;
        public event JournalizeEventHandler Journalize;
        public event TraceEventHandler Trace;

        protected void OnAction(ActionEventArgs args)
        {
            if (Action != null)
                Action(this, args);
        }

        protected void OnChoice(ChoiceEventArgs args)
        {
            if (Choice != null)
                Choice(this, args);
        }

        protected void OnConfirmation(ConfirmationEventArgs args)
        {
            if (Confirmation != null)
                Confirmation(this, args);
        }

        protected void OnDeliveryCheck(DeliveryCheckEventArgs args)
        {
            if (DeliveryCheck != null)
                DeliveryCheck(this, args);
        }

        protected void OnErrorOccurred(ErrorOccurredEventArgs args)
        {
            if (ErrorOccurred != null)
                ErrorOccurred(this, args);
        }

        protected void OnErrorCleared(ErrorClearedEventArgs args)
        {
            if (ErrorCleared != null)
                ErrorCleared(this, args);
        }

        protected void OnIrregularityDetected(IrregularityDetectedEventArgs args)
        {
            if (IrregularityDetected != null)
                IrregularityDetected(this, args);
        }

        protected void OnTerminalReadyChanged()
        {
            if (TerminalReadyChanged != null)
                TerminalReadyChanged(this, new EventArgs());
        }

        protected void OnJournalize(JournalizeEventArgs args)
        {
            if (Journalize != null)
                Journalize(this, args);
        }

        protected void OnTrace(TraceLevel level, string format, params object[] args)
        {
            if (Trace != null)
                Trace(this, new TraceEventArgs(String.Format(CultureInfo.InvariantCulture, format, args), level));
        }

        #endregion
    }
}
