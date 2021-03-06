﻿//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Web.Redis.Tests
{
    public class ChangeTrackingSessionStateItemCollectionTests
    {
        [Fact]
        public void SetItem_NewItem()
        {
            ChangeTrackingSessionStateItemCollection items = Utility.GetChangeTrackingSessionStateItemCollection();
            items["key"] = "value";
            items[0] = "value2";
            Assert.Single(items);
            Assert.Single(items.GetModifiedKeys());
            Assert.Single(items.innerCollection);
        }

        [Fact]
        public void Remove_Successful()
        {
            ChangeTrackingSessionStateItemCollection items = Utility.GetChangeTrackingSessionStateItemCollection();
            items["key"] = "value";
            Assert.Single(items);
            Assert.Empty(items.GetDeletedKeys());
            Assert.Single(items.GetModifiedKeys());
            items.Remove("key");
            Assert.Empty(items);
            Assert.Single(items.GetDeletedKeys());
            Assert.Empty(items.GetModifiedKeys());
        }

        [Fact]
        public void Remove_WrongKey()
        {
            ChangeTrackingSessionStateItemCollection items = Utility.GetChangeTrackingSessionStateItemCollection();
            items["key"] = "value";
            Assert.Single(items);
            Assert.Empty(items.GetDeletedKeys());
            Assert.Single(items.GetModifiedKeys());
            items.Remove("key1");
            Assert.Single(items);
            Assert.Empty(items.GetDeletedKeys());
            Assert.Single(items.GetModifiedKeys());
        }

        [Fact]
        public void RemoveAt_Successful()
        {
            ChangeTrackingSessionStateItemCollection items = Utility.GetChangeTrackingSessionStateItemCollection();
            items["key"] = "value";
            Assert.Single(items);
            Assert.Empty(items.GetDeletedKeys());
            Assert.Single(items.GetModifiedKeys());
            items.RemoveAt(0);
            Assert.Empty(items);
            Assert.Single(items.GetDeletedKeys());
            Assert.Empty(items.GetModifiedKeys());
        }

        [Fact]
        public void RemoveAt_WrongKeyIndex()
        {
            ChangeTrackingSessionStateItemCollection items = Utility.GetChangeTrackingSessionStateItemCollection();
            items["key"] = "value";
            Assert.Single(items);
            Assert.Empty(items.GetDeletedKeys());
            Assert.Single(items.GetModifiedKeys());
            Assert.Throws<ArgumentOutOfRangeException>(() => items.RemoveAt(1));
        }

        [Fact]
        public void Clear_EmptyCollection()
        {
            ChangeTrackingSessionStateItemCollection items = Utility.GetChangeTrackingSessionStateItemCollection();
            Assert.Empty(items);
            Assert.Empty(items.GetDeletedKeys());
            Assert.Empty(items.GetModifiedKeys());
            items.Clear();
            Assert.Empty(items);
            Assert.Empty(items.GetDeletedKeys());
            Assert.Empty(items.GetModifiedKeys());
        }
        
        [Fact]
        public void Clear_Successful()
        {
            ChangeTrackingSessionStateItemCollection items = Utility.GetChangeTrackingSessionStateItemCollection();
            items["key1"] = "value1";
            items["key2"] = "value2";
            items["key3"] = "value3";
            Assert.Equal(3, items.Count);
            Assert.Empty(items.GetDeletedKeys());
            Assert.Equal(3, items.GetModifiedKeys().Count);
            items.Clear();
            Assert.Empty(items);
            Assert.Equal(3, items.GetDeletedKeys().Count);
            Assert.Empty(items.GetModifiedKeys());
        }

        [Fact]
        public void Dirty_SetTrue()
        {
            ChangeTrackingSessionStateItemCollection items = Utility.GetChangeTrackingSessionStateItemCollection();
            items["key1"] = "value1";
            items["key2"] = "value2";
            items["key3"] = "value3";
            Assert.Equal(3, items.Count);
            Assert.Empty(items.GetDeletedKeys());
            Assert.Equal(3, items.GetModifiedKeys().Count);
            items.Dirty = true;
            Assert.Equal(3, items.Count);
            Assert.Empty(items.GetDeletedKeys());
            Assert.Equal(3, items.GetModifiedKeys().Count);
        }

        [Fact]
        public void Dirty_SetFalse()
        {
            ChangeTrackingSessionStateItemCollection items = Utility.GetChangeTrackingSessionStateItemCollection();
            items["key1"] = "value1";
            items["key2"] = "value2";
            items["key3"] = "value3";
            Assert.Equal(3, items.Count);
            Assert.Empty(items.GetDeletedKeys());
            Assert.Equal(3, items.GetModifiedKeys().Count);
            items.Dirty = false;
            Assert.Equal(3, items.Count);
            Assert.Empty(items.GetDeletedKeys());
            Assert.Empty(items.GetModifiedKeys());
        }

        [Fact]
        public void InsertRemoveUpdate_Sequence()
        {
            // Initial insert to set up value
            ChangeTrackingSessionStateItemCollection items = Utility.GetChangeTrackingSessionStateItemCollection();
            items["key1"] = "value1";
            Assert.Equal("value1", items[0]);
            Assert.True(items.Dirty);
            items.Dirty = false;

            // remove key
            items.Remove("key1");
            Assert.Empty(items);
            Assert.Single(items.GetDeletedKeys());
            Assert.Empty(items.GetModifiedKeys());
            
            // in same transaction insert same key than it should be update
            items["key1"] = "value1";
            Assert.Single(items);
            Assert.Empty(items.GetDeletedKeys());
            Assert.Single(items.GetModifiedKeys());
        }

        [Fact]
        public void MutableObject_FetchMarksDirty()
        {
            // Initial insert to set up value
            ChangeTrackingSessionStateItemCollection items = Utility.GetChangeTrackingSessionStateItemCollection();
            items["key1"] = new StringBuilder("value1");
            items.Dirty = false;
            Assert.Single(items);
            Assert.False(items.Dirty);
            Assert.Empty(items.GetModifiedKeys());
            Assert.Empty(items.GetDeletedKeys());
            
            // update value 
            StringBuilder sb = (StringBuilder)items["key1"];
            Assert.Single(items);
            Assert.Single(items.GetModifiedKeys());
        }

        [Fact]
        public void ImmutableObject_FetchDoNotMarksDirty()
        {
            // Initial insert to set up value
            ChangeTrackingSessionStateItemCollection items = Utility.GetChangeTrackingSessionStateItemCollection();
            items["key1"] = "value1";
            items["key2"] = 10;
            items.Dirty = false;
            Assert.Equal(2, items.Count);
            Assert.False(items.Dirty);
            Assert.Empty(items.GetModifiedKeys());
            Assert.Empty(items.GetDeletedKeys());

            // update value 
            string key1 = (string) items["key1"];
            int key2 = (int)items["key2"];
            Assert.Equal(2, items.Count);
            Assert.Empty(items.GetModifiedKeys());
        }

        [Fact]
        public void DuplicateWithDifferentCase()
        {
            // Initial insert to set up value
            ChangeTrackingSessionStateItemCollection items = Utility.GetChangeTrackingSessionStateItemCollection();
            items["Test"] = "v1";
            items.Dirty = false;

            foreach (string key in items.Keys)
            {
                Assert.Equal("Test", key);
                Assert.NotEqual("TEST", key);
                Assert.NotEqual("test", key);
            }
            
            Assert.Equal("v1", (string)items["Test"]);
            Assert.Equal("v1", (string)items["TEST"]);
            Assert.Equal("v1", (string)items["test"]);

            items["TEST"] = "v2";
            foreach (string key in items.Keys)
            {
                Assert.Equal("Test", key);
                Assert.NotEqual("TEST", key);
                Assert.NotEqual("test", key);
            }

            Assert.Equal("v2", (string)items["Test"]);
            Assert.Equal("v2", (string)items["TEST"]);
            Assert.Equal("v2", (string)items["test"]);
        }

        [Fact]
        public void Dirty_AfterLazyDeserialization()
        {
            RedisUtility utility = new RedisUtility(Utility.GetDefaultConfigUtility());
            ChangeTrackingSessionStateItemCollection items = Utility.GetChangeTrackingSessionStateItemCollection();
            items.SetDataWithoutUpdatingModifiedKeys("Test", utility.GetBytesFromObject("v1"));
            items.Dirty = false;

            Assert.False(items.Dirty);
            // read operation should not change value of Dirty
            var val = items["Test"];
            Assert.False(items.Dirty);

            items["Test"] = "v2";
            Assert.True(items.Dirty);
        }

        [Fact]
        public void Keys_AfterLazyDeserialization()
        {
            RedisUtility utility = new RedisUtility(Utility.GetDefaultConfigUtility());
            ChangeTrackingSessionStateItemCollection items = Utility.GetChangeTrackingSessionStateItemCollection();
            items.SetDataWithoutUpdatingModifiedKeys("k1", utility.GetBytesFromObject("v1"));
            items.SetDataWithoutUpdatingModifiedKeys("k2", utility.GetBytesFromObject("v2"));

            try
            {
                foreach (string key in items.Keys)
                {
                    // read operation should not change value items.Keys
                    var val = items[key];
                }
            }
            catch (System.InvalidOperationException)
            {
                Assert.False(true, "Reading value from collection should not change it");
            }
        }

        [Fact]
        public void GetEnumerator_AfterLazyDeserialization()
        {
            RedisUtility utility = new RedisUtility(Utility.GetDefaultConfigUtility());
            ChangeTrackingSessionStateItemCollection items = Utility.GetChangeTrackingSessionStateItemCollection();
            items.SetDataWithoutUpdatingModifiedKeys("k1", utility.GetBytesFromObject("v1"));
            items.SetDataWithoutUpdatingModifiedKeys("k2", utility.GetBytesFromObject("v2"));

            try
            {
                foreach (string key in items)
                {
                    // read operation should not change value items.Keys
                    var val = items[key];
                }
            }
            catch (System.InvalidOperationException)
            {
                Assert.False(true, "Reading value from collection should not change it");
            }
        }

        [Fact]
        public void Count_AfterLazyDeserialization()
        {
            RedisUtility utility = new RedisUtility(Utility.GetDefaultConfigUtility());
            ChangeTrackingSessionStateItemCollection items = Utility.GetChangeTrackingSessionStateItemCollection();
            items.SetDataWithoutUpdatingModifiedKeys("k1", utility.GetBytesFromObject("v1"));
            items.SetDataWithoutUpdatingModifiedKeys("k2", utility.GetBytesFromObject("v2"));

            Assert.Equal(2, items.Count);
            items["k3"] = "v3";
            Assert.Equal(3, items.Count);
        }

        [Fact]
        public void NullActualValue()
        {
            RedisUtility utility = new RedisUtility(Utility.GetDefaultConfigUtility());
            ChangeTrackingSessionStateItemCollection items = Utility.GetChangeTrackingSessionStateItemCollection();
            items.SetDataWithoutUpdatingModifiedKeys("k1", null);
            
            Assert.Single(items);
            Assert.Null(items["k1"]);
        }
    }
}
