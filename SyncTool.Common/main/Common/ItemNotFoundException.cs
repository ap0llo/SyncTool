// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;

namespace SyncTool.Common
{
    [Serializable]
    public class ItemNotFoundException : Exception
    {
        /// <summary>
        /// Gets the name of the item that was not found
        /// </summary>
        public string ItemName { get; }


        public ItemNotFoundException(string itemName) : base($"An item named '{itemName}' was not found")
        {
            if (itemName == null)
            {
                throw new ArgumentNullException(nameof(itemName));
            }

            this.ItemName = itemName;
        }
    }
}