namespace Simple.Owin.Hosting
{
    internal interface IOwinHostService
    {
        void Configure(OwinHostContext context);
    }
}