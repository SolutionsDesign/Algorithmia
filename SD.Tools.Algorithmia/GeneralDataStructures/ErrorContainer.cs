////////////////////////////////////////////////////////////////////////////////////////////
// Core Class definitions for the Application Core assembly of LLBLGen Pro.
// (c) 2002-2009 Solutions Design, all rights reserved.
// http://www.llblgen.com/
// 
// THIS IS NOT OPEN SOURCE SOFTWARE OF ANY KIND. 
// 
// Designed and developed by Frans Bouma.
///////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using SD.Tools.BCLExtensions.CollectionsRelated;

namespace SD.Tools.Algorithmia.GeneralDataStructures
{
	/// <summary>
	/// Simple class which is used as a container for error information for IDataErrorInfo implementations. 
	/// </summary>
	/// <remarks>Instead of doing housekeeping of error info in every class which implements IDataErrorInfo, you can use an instance of
	/// this class to do it for you. Simply set the errors in this class and retrieve the error info in the IDataErrorInfo implementation of your
	/// own class.</remarks>
	public class ErrorContainer : IDataErrorInfo
	{
		#region Class Member Declarations
		private string _dataErrorString;
		private readonly Dictionary<string, Pair<string, bool>> _errorPerProperty;		// value: Pair.Value1 = errorcode, Pair.Value2 = flag if error is soft (true) or not (false). Soft errors are removed after they're read.
		private readonly string _defaultError;
		#endregion


		/// <summary>
		/// Initializes a new instance of the <see cref="ErrorContainer"/> class.
		/// </summary>
		/// <param name="defaultError">The default error message to return by IDataErrorInfo.Error.</param>
		public ErrorContainer(string defaultError)
		{
			_dataErrorString = string.Empty;
			_errorPerProperty = new Dictionary<string, Pair<string, bool>>();
			_defaultError = defaultError;
		}


		/// <summary>
		/// Clears the errors contained in this container
		/// </summary>
		public void ClearErrors()
		{
			_dataErrorString = string.Empty;
			_errorPerProperty.Clear();
		}


		/// <summary>
		/// Gets all property names with errors stored in this errorcontainer. Use the names to index into this container to obtain the error for this particular
		/// property. Properties with an empty string as error are ignored.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> GetAllPropertyNamesWithErrors()
		{
			foreach(KeyValuePair<string, Pair<string, bool>> pair in _errorPerProperty)
			{
				if(!string.IsNullOrEmpty(pair.Value.Value1))
				{
					yield return pair.Key;
				}
			}
		}


		/// <summary>
		/// Converts all property-error pairs to a new-line delimited string .
		/// </summary>
		/// <param name="prefixEachLineWithDash">if set to <see langword="true"/> each line is prefixed with a '-'</param>
		/// <returns></returns>
		public string ConvertToNewLineDelimitedList(bool prefixEachLineWithDash)
		{
			StringBuilder builder = new StringBuilder();
			bool first = true;
			foreach(string propertyName in GetAllPropertyNamesWithErrors())
			{
				string error = this[propertyName];
				if(!first)
				{
					builder.Append(Environment.NewLine);
				}
				if(prefixEachLineWithDash)
				{
					builder.Append(" - ");
				}
				builder.AppendFormat("{0}: {1}", propertyName, error);
				first = false;
			}
			return builder.ToString();
		}


		/// <summary>
		/// Sets the property error. If an empty errorDescription is passed in, the error information is cleared. Always logs a hard error.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="errorDescription">The error description.</param>
		public void SetPropertyError(string propertyName, string errorDescription)
		{
			SetPropertyError(propertyName, errorDescription, false);
		}


		/// <summary>
		/// Sets the property error. If an empty errorDescription is passed in, the error information is cleared.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="errorDescription">The error description.</param>
		/// <param name="isSoftError">if set to <see langword="true"/> the error is considered 'soft', which means it's cleared after it's been read.</param>
		public void SetPropertyError(string propertyName, string errorDescription, bool isSoftError)
		{
			if(errorDescription.Length <= 0)
			{
				_errorPerProperty.Remove(propertyName);
			}
			else
			{
				_errorPerProperty[propertyName] = new Pair<string, bool>(errorDescription, isSoftError);
				_dataErrorString = _defaultError;
			}
			if(_errorPerProperty.Count <= 0)
			{
				_dataErrorString = string.Empty;
			}
		}


		/// <summary>
		/// Appends the property error to an existing error for that property. Appends a newline first.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="errorDescription">The error description.</param>
		/// <remarks>To remove an error message for a property, call SetPropertyError with an empty string as error message.
		/// Always logs a hard error</remarks>
		public void AppendPropertyError(string propertyName, string errorDescription)
		{
			AppendPropertyError(propertyName, errorDescription, Environment.NewLine, false);
		}


		/// <summary>
		/// Appends the property error to an existing error for that property. Appends a newline first.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="errorDescription">The error description.</param>
		/// <param name="isSoftError">if set to <see langword="true"/> the error is considered 'soft', which means it's cleared after it's been read.</param>
		/// <remarks>To remove an error message for a property, call SetPropertyError with an empty string as error message.
		/// Overwrites the existing isSoftError flag value of the existing error of the property with the value specified in isSoftError</remarks>
		public void AppendPropertyError(string propertyName, string errorDescription, bool isSoftError)
		{
			AppendPropertyError(propertyName, errorDescription, Environment.NewLine, isSoftError);
		}


		/// <summary>
		/// Appends the property error to an existing error for that property. Appends the lineSeparator to the existing error first. 
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="errorDescription">The error description.</param>
		/// <param name="lineSeparator">The line separator to append to the existing error message.</param>
		/// <remarks>To remove an error message for a property, call SetPropertyError with an empty string as error message.
		/// Always logs a hard error</remarks>
		public void AppendPropertyError(string propertyName, string errorDescription, string lineSeparator)
		{
			AppendPropertyError(propertyName, errorDescription, lineSeparator, false);
		}


		/// <summary>
		/// Appends the property error to an existing error for that property. Appends the lineSeparator to the existing error first. 
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="errorDescription">The error description.</param>
		/// <param name="lineSeparator">The line separator to append to the existing error message.</param>
		/// <param name="isSoftError">if set to <see langword="true"/> the error is considered 'soft', which means it's cleared after it's been read.</param>
		/// <remarks>To remove an error message for a property, call SetPropertyError with an empty string as error message.
		/// Overwrites the existing isSoftError flag value of the existing error of the property with the value specified in isSoftError</remarks>
		public void AppendPropertyError(string propertyName, string errorDescription, string lineSeparator, bool isSoftError)
		{
			if(string.IsNullOrEmpty(errorDescription))
			{
				return;
			}
			if(string.IsNullOrEmpty(propertyName))
			{
				throw new ArgumentException("propertyName is empty or null");
			}
			string existingError = this[propertyName];
			if(existingError.Length > 0)
			{
				existingError = existingError + lineSeparator;
			}
			SetPropertyError(propertyName, existingError + errorDescription, isSoftError);
		}


		#region IDataErrorInfo Members
		/// <summary>
		/// Gets an error message indicating what is wrong with this object.
		/// </summary>
		/// <returns>
		/// An error message indicating what is wrong with this object. The default is an empty string ("").
		/// </returns>
		public string Error
		{
			get { return _dataErrorString; }
		}

		/// <summary>
		/// Gets the <see cref="System.String"/> with the specified column name.
		/// </summary>
		public string this[string columnName]
		{
			get
			{
				Pair<string, bool> loggedErrorInfo = _errorPerProperty.GetValue(columnName);
				if(loggedErrorInfo==null)
				{
					return string.Empty;
				}
				if(loggedErrorInfo.Value2)
				{
					// soft error, so reset it
					SetPropertyError(columnName, string.Empty);
				}
				return loggedErrorInfo.Value1;
			}
		}

		#endregion
	}
}
