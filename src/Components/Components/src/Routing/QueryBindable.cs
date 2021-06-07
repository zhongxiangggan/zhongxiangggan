namespace Microsoft.AspNetCore.Components.Routing
{
    /// <summary>
    /// </summary>
    public readonly struct QueryBindable<T>
    {
        private readonly NavigationManager _navigationManager;
        private readonly string _name;
        private readonly bool _replace;

        internal QueryBindable(NavigationManager navigationManager, string name, bool replace)
        {
            _navigationManager = navigationManager;
            _name = name;
            _replace = replace;
        }

        /// <summary>
        /// </summary>
        public T? Value
        {
            get => _navigationManager.GetQueryParameter<T>(_name);
            set => _navigationManager.SetQueryParameter(_name, value, _replace);
        }
    }
}
