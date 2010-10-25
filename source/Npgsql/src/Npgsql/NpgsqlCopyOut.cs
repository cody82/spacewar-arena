// Npgsql.NpgsqlCopyOut.cs
//
// Author:
//     Kalle Hallivuori <kato@iki.fi>
//
//    Copyright (C) 2007 The Npgsql Development Team
//    npgsql-general@gborg.postgresql.org
//    http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
// 
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.


using System.IO;

namespace Npgsql
{
	/// <summary>
	/// Represents a PostgreSQL COPY TO STDOUT operation with a corresponding SQL statement
	/// to execute against a PostgreSQL database
	/// and an associated stream used to write results to (if provided by user)
	/// or for reading the results (when generated by driver).
	/// Eg. new NpgsqlCopyOut("COPY (SELECT * FROM mytable) TO STDOUT", connection, streamToWrite).Start();
	/// </summary>
	public class NpgsqlCopyOut
	{
		private readonly NpgsqlConnector _context;
		private readonly NpgsqlCommand _cmd;
		private Stream _copyStream;
		private bool _disposeCopyStream; // user did not provide stream, so reset it after use

		/// <summary>
		/// Creates NpgsqlCommand to run given query upon Start(), after which CopyStream provides data from database as requested in the query.
		/// </summary>
		public NpgsqlCopyOut(string copyOutQuery, NpgsqlConnection conn)
			: this(new NpgsqlCommand(copyOutQuery, conn), conn)
		{
		}

		/// <summary>
		/// Given command is run upon Start(), after which CopyStream provides data from database as requested in the query.
		/// </summary>
		public NpgsqlCopyOut(NpgsqlCommand cmd, NpgsqlConnection conn)
			: this(cmd, conn, null)
		{
		}

		/// <summary>
		/// Given command is executed upon Start() and all requested copy data is written to toStream immediately.
		/// </summary>
		public NpgsqlCopyOut(NpgsqlCommand cmd, NpgsqlConnection conn, Stream toStream)
		{
			_context = conn.Connector;
			_cmd = cmd;
			_copyStream = toStream;
		}

		/// <summary>
		/// Returns true if the connection is currently reserved for this operation.
		/// </summary>
		public bool IsActive
		{
			get
			{
				return
					_context != null && _context.CurrentState is NpgsqlCopyOutState && _context.Mediator.CopyStream == _copyStream;
			}
		}

		/// <summary>
		/// The stream provided by user or generated upon Start()
		/// </summary>
		public Stream CopyStream
		{
			get { return _copyStream; }
		}

		/// <summary>
		/// The Command used to execute this copy operation.
		/// </summary>
		public NpgsqlCommand NpgsqlCommand
		{
			get { return _cmd; }
		}

		/// <summary>
		/// Returns true if this operation is currently active and in binary format.
		/// </summary>
		public bool IsBinary
		{
			get { return IsActive && _context.CurrentState.CopyFormat.IsBinary; }
		}

		/// <summary>
		/// Returns true if this operation is currently active and field at given location is in binary format.
		/// </summary>
		public bool FieldIsBinary(int fieldNumber)
		{
			return IsActive && _context.CurrentState.CopyFormat.FieldIsBinary(fieldNumber);
		}

		/// <summary>
		/// Returns number of fields if this operation is currently active, otherwise -1
		/// </summary>
		public int FieldCount
		{
			get { return IsActive ? _context.CurrentState.CopyFormat.FieldCount : -1; }
		}

		/// <summary>
		/// Command specified upon creation is executed as a non-query.
		/// If CopyStream is set upon creation, all copy data from server will be written to it, and operation will be finished immediately.
		/// Otherwise the CopyStream member can be used for reading copy data from server until no more data is available.
		/// </summary>
		public void Start()
		{
			if (_context.CurrentState is NpgsqlReadyState)
			{
				_context.Mediator.CopyStream = _copyStream;
				_cmd.ExecuteBlind();
				_disposeCopyStream = _copyStream == null;
				_copyStream = _context.Mediator.CopyStream;
				if (_copyStream == null && ! (_context.CurrentState is NpgsqlReadyState))
				{
					throw new NpgsqlException("Not a COPY OUT query: " + _cmd.CommandText);
				}
			}
			else
			{
				throw new NpgsqlException("Copy can only start in Ready state, not in " + _context.CurrentState);
			}
		}

		/// <summary>
		/// Faster alternative to using the generated CopyStream.
		/// </summary>
		public byte[] Read
		{
			get { return IsActive ? ((NpgsqlCopyOutStream) _copyStream).Read() : null; }
		}

		/// <summary>
		/// Flush generated CopyStream at once. Effectively reads and discard all the rest of copy data from server.
		/// </summary>
		public void End()
		{
			if (_context != null)
			{
				bool wasActive = IsActive;
				if (wasActive)
				{
					if (_copyStream is NpgsqlCopyOutStream)
					{
						_copyStream.Close();
					}
					else
					{
						while (_context.CurrentState.GetCopyData(_context) != null)
						{
							; // flush rest
						}
					}
				}
				if (_context.Mediator.CopyStream == _copyStream)
				{
					_context.Mediator.CopyStream = null;
					if (_disposeCopyStream)
					{
						_copyStream = null;
					}
				}
			}
		}
	}
}