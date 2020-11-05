namespace UPS.Core
{
    public enum ApiControllerName
    {
        users,
        posts,
        products,
        categories,
    }
    public enum HandlerActionType
    {
        Create,
        Update,
        Delete,
        Read,
        ReadPage,
        SetFilter,
        Export
    }
    public enum Gender
    {
        Male,
        Female
    }
    public enum Status
    {
        Active,
        Inactive
    }
}
