using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoDrop
{
    class clsEDP
    {
        public const int MAX_SIZE = 65535;
        public const int HEADER_SIZE = 6;

        private byte _nCommand = 0;
        public byte m_nCommand => _nCommand;

        private byte _nParam = 0;
        public byte m_nParam => _nParam;

        private int _nDataLength = 0;
        public int m_nDataLength => _nDataLength;

        private byte[] _abMessageData = Array.Empty<byte>();
        public byte[] m_abMessageData => _abMessageData;

        private byte[] _abMoreData = Array.Empty<byte>();
        public byte[] m_abMoreData => _abMoreData;

        // Constructor for parsing received buffer
        public clsEDP(byte[] abBuffer)
        {
            if (abBuffer == null || abBuffer.Length < HEADER_SIZE)
                return;

            using (var ms = new MemoryStream(abBuffer))
            using (var br = new BinaryReader(ms))
            {
                _nCommand = br.ReadByte();
                _nParam = br.ReadByte();
                _nDataLength = br.ReadInt32(); // <-- little-endian by default

                if (abBuffer.Length - HEADER_SIZE >= _nDataLength && _nDataLength > 0)
                    _abMessageData = br.ReadBytes(_nDataLength);

                int remaining = (int)(abBuffer.Length - HEADER_SIZE - _nDataLength);
                if (remaining > 0)
                    _abMoreData = br.ReadBytes(remaining);
            }
        }

        // Constructor for building packets to send
        public clsEDP(byte nCmd, byte nParam, byte[] abMsg)
        {
            _nCommand = nCmd;
            _nParam = nParam;
            _abMessageData = abMsg;
            _nDataLength = _abMessageData.Length;
        }

        public byte[] fnabGetBytes()
        {
            try
            {
                using (var ms = new MemoryStream())
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write(_nCommand);
                    bw.Write(_nParam);
                    bw.Write(_nDataLength);
                    bw.Write(_abMessageData);

                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "fnabGetBytes()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return Array.Empty<byte>();
            }
        }

        public (byte nCommand, byte nParam, int nLength, byte[] abMsg) fnGetMsg()
            => (_nCommand, _nParam, _nDataLength, _abMessageData);

        public static (byte nCommand, byte nParam, int nLength) fnGetHeader(byte[] abBuffer)
        {
            if (abBuffer == null || abBuffer.Length < HEADER_SIZE)
                return (0, 0, 0);

            byte nCommand = abBuffer[0];
            byte nParam = abBuffer[1];
            int nLength = BitConverter.ToInt32(abBuffer, 2);

            return (nCommand, nParam, nLength);
        }
    }
}
