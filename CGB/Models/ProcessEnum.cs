namespace CGB.Models
{
    public enum ProcessEnum
    {
        Start,
        Login,
        EnterLoginPassword,
        EnterCaptcha,
        SubmitLogin,
        MainPage,
        
        // Autopay
        NavigateAutopayMenu,
        NavigateAutopaySubMenu,
        TransferPaymentInfo,
        SubmitPaymentInfo,
        CheckPaymentAmount,
        GetPaymentVerification,
        TransferingPaymentInfo,
        EnterATMPassword,
        SubmitATMPassword,
        EnterUkey,
        PressUKey,
        CheckTransferring,
        Transferring,
        GetTransferResult,
        Transferred,
        FailedTransfer,
        NextOrder,

        // Balance
        NavigateBalanceMainMenu,
        NavigateMyAccountMenu,
        NavigateBalance,
        GetAccountStatusAndBalance,
        SaveBalance,

        //Collection
        NavigateCollectionMainMenu,
        NavigateMyAccountCollectionMenu,
        NavigateCollection,
        SelectDate,
        GetCollectionList,
        NavigateCollectionDetails,
        GetCollectionDetails,
        NextPage,
        SaveCollection,
        FinishedCollection,

        Done
    }
}