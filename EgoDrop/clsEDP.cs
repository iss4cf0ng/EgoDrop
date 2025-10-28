using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoDrop
{
    internal class clsEDP
    {
        //Header
        public const int HEADER_SIZE = 6;
        private byte _nCommand = 0;
        public byte m_nCommand { get { return _nCommand; } }
        private byte _nParam = 0;
        public byte m_nParam { get { return _nParam; } }
        private int _nDataLength = 0;
        public int m_nDataLength { get { return _nDataLength; } }

        //Data Message
        private byte[] _abMessageData = new byte[0];
        private byte[] m_abMessageData = new byte[0];
        private byte[] _abMoreData = new byte[0];
        private byte[] m_abMoreData = new byte[0];

        //Constructor-1
        public clsEDP(byte[] abBuffer)
        {
            //Validate buffer.
            if (abBuffer == null || abBuffer.Length < HEADER_SIZE)
                return;

            //Handle buffer
            using (MemoryStream ms = new MemoryStream(abBuffer))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    _nCommand = br.ReadByte();
                    _nParam = br.ReadByte();
                    _nDataLength = br.ReadInt32();

                    if (abBuffer.Length - HEADER_SIZE >= m_nDataLength)
                        _abMessageData = br.ReadBytes(_nDataLength);
                    if (abBuffer.Length - HEADER_SIZE - m_nDataLength > 0)
                        _abMoreData = br.ReadBytes(abBuffer.Length - HEADER_SIZE - m_nDataLength);
                }
            }
        }

        //Constructor-2
        public clsEDP(byte nCmd, byte nParam, byte[] abMsg)
        {
            _nCommand = nCmd;
            _nParam = nParam;
            _abMessageData = abMsg;

            _nDataLength = abMsg.Length;
        }

        public byte[] fnabGetBytes()
        {
            try
            {
                byte[] abBuffer = { };
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter bw = new BinaryWriter(ms))
                    {
                        bw.Write(m_nCommand);
                        bw.Write(m_nParam);
                        bw.Write(m_nDataLength);
                        bw.Write(m_abMessageData);

                        abBuffer = ms.ToArray();
                    }
                }

                return abBuffer;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "fnabGetBytes()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new byte[0];
            }
        }

        public (byte nCommand, byte nParam, int nLength, byte[] abMsg) fnGetMsg()
        {
            (
                byte nCommand,
                byte nParam,
                int nLength,
                byte[] abMsg
            ) ret = (
                m_nCommand,
                m_nParam,
                m_nDataLength,
                m_abMessageData
            );

            return ret;
        }

        public static (byte nCommand, byte nParam, int nLength) fnGetHeader(byte[] abBuffer)
        {
            (
                byte nCommand,
                byte nParam,
                int nLength
            ) ret = (0, 0, 0);

            ret.nCommand = abBuffer[0];
            ret.nParam = abBuffer[1];
            ret.nLength = BitConverter.ToInt32(abBuffer, 2);

            return ret;
        }
    }
}
