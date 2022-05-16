namespace XRL.Wish {
    [HasWishCommand]
    public static partial class ManticoreWishes {
        [WishCommand(Command = "updatebody")]
        public static void UpdateBody()
        {
            The.Player.Body.UpdateBodyParts(0);
        }
    }
}