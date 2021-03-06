//  Copyright (C) 2017, The Duplicati Team
//  http://www.duplicati.com, info@duplicati.com
//
//  This library is free software; you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 2.1 of the
//  License, or (at your option) any later version.
//
//  This library is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//  Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public
//  License along with this library; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
using System;
using System.Linq;

namespace Duplicati.Library.Logging
{
    /// <summary>
    /// Internal class for keeping log instance relations
    /// </summary>
    internal class LogScope : IDisposable
    {
        /// <summary>
        /// The unique ID of this log instance
        /// </summary>
        public readonly string InstanceID = Guid.NewGuid().ToString();

        /// <summary>
        /// The log instance assigned to this scope
        /// </summary>
        private readonly ILogDestination m_log;

        /// <summary>
        /// The log filter
        /// </summary>
        private readonly ILogFilter m_filter;

        /// <summary>
        /// The log scope parent
        /// </summary>
        public readonly LogScope Parent;

        /// <summary>
        /// A flag indicating if the scope is disposed
        /// </summary>
        private bool m_isDisposed = false;

        /// <summary>
        /// A flag indicating if this is an isolating scope
        /// </summary>
        public readonly bool IsolatingScope;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Duplicati.Library.Logging.LogWrapper"/> class.
        /// </summary>
        /// <param name="self">The log instance to wrap.</param>
        /// <param name="filter">The log filter to use</param>
        /// <param name="parent">The parent scope</param>
        /// <param name="isolatingScope">A flag indicating if the scope is an isolating scope</param>
        public LogScope(ILogDestination self, ILogFilter filter, LogScope parent, bool isolatingScope)
        {
            Parent = parent;

            m_log = self;
            m_filter = filter;
            IsolatingScope = isolatingScope;

            if (parent != null)
                Logging.Log.StartScope(this);
        }

        /// <summary>
        /// The function called when a message is logged
        /// </summary>
        /// <param name="entry">The log entry</param>
        public void WriteMessage(LogEntry entry)
        {
            if (m_log != null && (m_filter == null || m_filter.Accepts(entry)))
                m_log.WriteMessage(entry);
        }

        /// <summary>
        /// Releases all resource used by the <see cref="T:Duplicati.Library.Logging.LogScope"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="T:Duplicati.Library.Logging.LogScope"/>. The <see cref="Dispose"/> method leaves the
        /// <see cref="T:Duplicati.Library.Logging.LogScope"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the
        /// <see cref="T:Duplicati.Library.Logging.LogScope"/> so the garbage collector can reclaim the memory that the
        /// <see cref="T:Duplicati.Library.Logging.LogScope"/> was occupying.</remarks>
        public void Dispose()
        {
            if (!m_isDisposed && Parent != null)
            {
                Logging.Log.CloseScope(this);
                m_isDisposed = true;
            }
        }
    }
}
