namespace XRL.Wish {
    [HasWishCommand]
    public static class Wishes {
        [WishCommand(Command = "updatebody")]
        public static void UpdateBody()
        {
            The.Player.Body.UpdateBodyParts(0);
        }
    }
}