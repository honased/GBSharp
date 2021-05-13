using System;
using WebSocketSharp;

namespace GBSharp
{
    public class LinkCable
    {
        private WebSocket socket;
        private bool transferInProgress;
        private bool clockSpeed;
        private bool shiftClock;
        private byte[] data;
        private Gameboy _gameboy;

        private const bool ENABLED = false;

        public LinkCable(Gameboy gameboy)
        {
            if (ENABLED)
            {
                socket = new WebSocket("ws://127.0.0.1:8001/GBSharp");
                socket.OnMessage += Socket_OnMessage;
                socket.Connect();
            }
            _gameboy = gameboy;
            Reset();
        }

        private void Socket_OnMessage(object sender, MessageEventArgs e)
        {
            lock(this)
            {
                if (!shiftClock)
                {
                    socket.Send(data);
                }

                data[0] = e.RawData[0];
                transferInProgress = false;

                _gameboy.Mmu.SetInterrupt(Interrupts.Serial);
            }
        }

        internal void Reset()
        {
            transferInProgress = false;
            clockSpeed = false;
            shiftClock = false;
            data = new byte[1];
        }

        public void WriteByte(int address, int value)
        {
            switch(address)
            {
                case 0xFF01:
                    lock(this)
                    {
                        data[0] = (byte)value;
                    }
                    break;

                case 0xFF02:
                    lock (this)
                    {
                        bool oldTransfer = transferInProgress;
                        transferInProgress = Bitwise.IsBitOn(value, 7);
                        clockSpeed = Bitwise.IsBitOn(value, 1);
                        shiftClock = Bitwise.IsBitOn(value, 0);
                        if (!oldTransfer && transferInProgress && shiftClock && ENABLED && socket.IsAlive)
                        {
                            socket.Send(data);
                        }
                    }
                    break;

                default:
                    Console.WriteLine("Can't read from address " + address.ToString() + " in LinkCable");
                    break;
            }
        }

        public int ReadByte(int address, int[] memory)
        {
            switch(address)
            {
                case 0xFF01:
                    return data[0];

                case 0xFF02:
                    return ((transferInProgress ? 1 : 0) << 7) | ((clockSpeed ? 1 : 0) << 1) | (shiftClock ? 1 : 0);
            }

            return memory[address - 0xFF00];
        }
    }
}
