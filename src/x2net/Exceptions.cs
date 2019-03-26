// Copyright (c) 2017-2019 Jae-jun Kang
// See the file LICENSE for details.

using System;

namespace x2net
{
    /// <summary>
    /// The exception that is thrown when a data stream is in an invalid format.
    /// </summary>
    public class InvalidEncodingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the InvalidEncodingException class.
        /// </summary>
        public InvalidEncodingException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the InvalidEncodingException class
        /// with the specified error message.
        /// </summary>
        public InvalidEncodingException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the InvalidEncodingException class
        /// with the specified error message and the reference to the inner
        /// exception that is the cause of this exception.
        /// </summary>
        public InvalidEncodingException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    /// <summary>
    /// The exception that is thrown when there is not enough resource to
    /// fulfill the request.
    /// </summary>
    public class OutOfResourceException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the OutOfResourceException class.
        /// </summary>
        public OutOfResourceException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the OutOfResourceException class with
        /// the specified error message.
        /// </summary>
        public OutOfResourceException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the OutOfResourceException class with
        /// the specified error message and the reference to the inner exception
        /// that is the cause of this exception.
        /// </summary>
        public OutOfResourceException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
