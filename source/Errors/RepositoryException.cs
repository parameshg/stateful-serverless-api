namespace Api.Errors
{
    public class RepositoryException : ApplicationException
    {
        public RepositoryException()
        {
        }

        public RepositoryException(string message)
            : base(message)
        {
        }
    }
}