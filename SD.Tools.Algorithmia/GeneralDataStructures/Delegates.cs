using System;

namespace SD.Tools.Algorithmia.GeneralDataStructures
{
	/// <summary>
	/// Delegate for the member value changed handler
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	public delegate void MemberValueElementChangedHandler(object sender, EventArgs e);
	/// <summary>
	/// Delegate for the member value removed handler
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	public delegate void MemberValueElementRemovedHandler(object sender, EventArgs e);
}