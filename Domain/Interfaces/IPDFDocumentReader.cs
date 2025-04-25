using System.IO;

namespace Domain.Interfaces
{
    public interface IPDFDocumentReader<T>
    {
        /// <summary>
        /// Reads a PDF document from a stream.
        /// </summary>
        /// <param name="fs">The stream containing the PDF data.</param>
        /// <returns>A loaded PDF document object.</returns>
        T Read(Stream fs);
    }
} 