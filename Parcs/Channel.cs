using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Reflection;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Security.Cryptography;

namespace Parcs
{
    public class Channel : IChannel
    {
        private BinaryReader _reader;
        private BinaryWriter _writer;
        private int _from;
        private static object _syncRoot = new object();

        public int From
        {
            get { return _from; }
            set { _from = value; }
        }

        private int _index = -1;

        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }
      
        public bool Works { get; private set; }

        public Channel(BinaryReader reader, BinaryWriter writer, bool works)//, int from, int index, bool works)
        {
            _reader = reader;
            _writer = writer;
            Works = works;
        }

        public Channel(BinaryReader reader, BinaryWriter writer, int pointNumber, int index, bool works)
            : this(reader, writer, true)
        {
            From = pointNumber;
            _index = index;
            Works = works;
        }

        public virtual void WriteData(dynamic data)
        {
            _writer.Write(data);
            _writer.Flush();
        }

        public virtual dynamic ReadData(Type type)
        {
            if (type == typeof(bool))
            {
                return _reader.ReadBoolean();
            }

            if (type == typeof(byte))
            {
                return _reader.ReadByte();
            }

            if (type == typeof(int))
            {
                return _reader.ReadInt32();
            }

            if (type == typeof(long))
            {
                return _reader.ReadInt64();
            }

            if (type == typeof(double))
            {
                double val = _reader.ReadDouble(); 
                return val;
            }

            if (type == typeof(string))
            {
                return _reader.ReadString();
            }

            return null;
        }

        public virtual void WriteObject(object obj)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, obj);
                memoryStream.ToArray();
                byte[] byteObject = memoryStream.ToArray();
                _writer.Write(byteObject.Length);
                _writer.Write(byteObject);
                // _writer.Flush();
            }

        }
        public object ReadObject()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return ReadObject(formatter);
        }

        public object ReadObject(Type type)
        {
            BinaryFormatter formatter = new BinaryFormatter { Binder = new MySerializationBinder(type) };
            return ReadObject(formatter);
        }

        protected virtual object ReadObject(BinaryFormatter formatter)
        {
            int numberOfBytes = _reader.ReadInt32();
            byte[] obj = _reader.ReadBytes(numberOfBytes);
            object o = null;
            using (MemoryStream memoryStream = new MemoryStream(obj))
            {
                o = formatter.Deserialize(memoryStream);
            }

            return o;
        }

        public virtual void WriteFile(string fullPath)
        {
            byte[] file = File.ReadAllBytes(fullPath);
            //_writer.Write((new FileInfo(fullPath)).Name);
            _writer.Write(file.Length);
            _writer.Write(file);
            _writer.Flush();
        }

        public virtual string ReadFile()//сохраняет файл в хранилище.
        {
            //string fileName = _reader.ReadString();
            int fileSize = _reader.ReadInt32();
            byte[] file = _reader.ReadBytes(fileSize);
            string fileName = GetHashedFileName(file);
            
            IsolatedStorageFile userFile = IsolatedStorageFile.GetUserStoreForAssembly();

            if (!userFile.FileExists(fileName))
            {
                lock (_syncRoot)
                {
                    if (!userFile.FileExists(fileName))
                    {
                        IsolatedStorageFileStream isolatedStream = new IsolatedStorageFileStream(fileName, FileMode.Create, userFile);
                        BinaryWriter writer = new BinaryWriter(isolatedStream);
                        writer.Write(file);

                        writer.Close();
                        isolatedStream.Close();
                    }
                }
            }

            return userFile.GetType().GetField("m_RootDir", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(userFile).ToString() + "\\" + fileName;
            
            //return isolatedStream.GetType().GetField("m_FullPath", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(isolatedStream).ToString();
        }

        public virtual void Close()
        {
            if (_reader != null)
            {
                _reader.Close();
            }

            if (_writer != null)
            {
                _writer.Close();
            }
        }

        private string GetHashedFileName(byte[] file)
        {
            return ByteArrayToString(GetHash(file));
        }

        private byte[] GetHash(byte[] data)
        {
            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                return sha1.ComputeHash(data);
            }
        }

        private string ByteArrayToString(byte[] byteArray)
        {
            StringBuilder hex = new StringBuilder(byteArray.Length * 2);
            foreach (byte b in byteArray)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }

        private class MySerializationBinder : SerializationBinder
        {
            private Type _type;

            public MySerializationBinder(Type type)
            {
                _type = type;
            }

            public override Type BindToType(string assemblyName, string typeName)
            {
                return _type;
            }
        }

    }
}
