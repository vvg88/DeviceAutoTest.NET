using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.DeviceAutoTest.ScriptExecution;
using NeuroSoft.DeviceAutoTest.BluetoothDevicesTest.Controls;
using NeuroSoft.Hardware.Devices;
using NeuroSoft.MathLib.Filters;

namespace NeuroSoft.DeviceAutoTest.BluetoothDevicesTest
{
    public partial class ProtocolPS8EXv2012
    {

        #region PS-8/EX-v.2012

        #region переменные
        public static byte myChannelsCount = 8 + 1;    // кол-во каналов измерения ЭКГ + канал дыхания
        public static int LocBufEnd = 0;        // указатель на конец локального кольцевого буфера
        public static int LocBufferLen = 0;     // длина локального приемного буфера

        /// <summary>
        /// колличество отсчетов ЭКГ в текущем пакете
        /// </summary>
        public static byte NumFramesInCurPacket = 0;

        /// <summary>
        /// номер первого отсчета ЭКГ в пакете
        /// </summary>
        public static int NumFirstADCFrame = 0;

        static byte InpDataCurType = 0;    // тип входящих данных
        static byte counter_ecg_pace = 0;  // счётчик отсчётов для pacemaker
        static byte indication_pace = 0;   // признак pacemaker на 8 отсчетов
        static byte counter_breath = 0; // счётчик отсчётов для канала дыхания
        static int LocBufferPosition;      // позиция начала данных в локальном приемном пункте
        public static int addressCounter = 0; // счетчик адреса в буфере входящих данных

        static Filter[] filtersBr;       // фильтр для канала дыхания
        /// <summary>
        /// Число принятых байт в секунду
        /// </summary>
        public static int ByteToSecond = 0;

        static int maxLenBuffer = 1024 * 1024;  // максимальная длина приёмного буфера
        /// <summary>
        /// буфер входящих данных
        /// </summary>
        static byte[] ringbuf = new byte[1024 * 1024];
        #endregion

        /// <summary>
        /// Начинает передачу.
        /// </summary>
        public static void BeginTransmit()
        {
            MonitoringData.Device8EXv2012.StartAsyncReading();
            addressCounter = 0; LocBufferLen = 0; LocBufEnd = 0;
            byte[] c_start = { 0xC0, 0x14, 0x07 }; // start monitoring
            MonitoringData.Device8EXv2012.Write(c_start, 0, 3);
        }
		/// <summary>
		/// Прекращает передачу.
		/// </summary>
        public static void StopTransmit()
        {
            byte[] c_start = { 0xC0, 0x15, 0x00 }; // stop monitoring
            MonitoringData.Device8EXv2012.Write(c_start, 0, 3);
            MonitoringData.Device8EXv2012.StopAsyncReading();
        }

        /// <summary>
        /// Читаем данные из потока данных по Bluetooth
        /// </summary>
        public static void ReadDeviceData(byte[] buffer, int count)
        {
            int i;
            byte SincCode;
            int cur_packet_size = 0; // размер пакета

            ByteToSecond += count;
            NumFramesInCurPacket = 0;

            // читаем приёмный буфер
            for (i = 0; i < count; i++)
            {
                if ((LocBufEnd) >= maxLenBuffer)
                {
                    LocBufEnd = 0;
                }
                ringbuf[LocBufEnd++] = buffer[i];
            }
            LocBufferLen += count;
            do
            {
                LocBufferPosition = addressCounter;
                // читаем синхро-код
                SincCode = ReadByteRingBuffer();
                // читаем тип пакета
                InpDataCurType = ReadByteRingBuffer();
                // читаем первый байт размера пакета
                cur_packet_size = ReadByteRingBuffer();
                // читаем второй байт размера пакета
                cur_packet_size |= ReadByteRingBuffer() << 8;
                if (SincCode != PacketType.BLUETOOTH_PACKET_BEGIN_MARKER)
                {
                    addressCounter = LocBufferPosition; // указываем на начало предыдущего непрочитанного пакета данных
                    LocBufEnd = LocBufferPosition;
                    goto finished;
                }
                if ((InpDataCurType != PacketType.PACKET_TYPE_MONITORING_DATA)
                    & (InpDataCurType != PacketType.PACKET_TYPE_ECG_PACE_DATA)
                    & (InpDataCurType != PacketType.PACKET_TYPE_PACE_DATA)
                    & (InpDataCurType != PacketType.PACKET_TYPE_BR_DATA)
                    & (InpDataCurType != PacketType.PACKET_TYPE_BATT_DATA)
                    & (InpDataCurType != PacketType.PACKET_TYPE_SING_PACE)
                    & (InpDataCurType != PacketType.PACKET_TYPE_PACE_PKT))
                {
                    LocBufEnd = 0;
                    addressCounter = 0;
                    LocBufferLen = 0;
                    goto finished;
                }
                // указываем на конец буфера
                addressCounter += cur_packet_size - 4;
                if (addressCounter >= maxLenBuffer)
                {
                    addressCounter -= maxLenBuffer;
                }
                // проверяем что пакет пришёл полностью
                if (ReadByteRingBuffer() != 0xC0)
                {
                    addressCounter = LocBufferPosition; // указываем на начало предыдущего непрочитанного пакета данных
                    goto finished;
                }
                // восстанавливаем указатель буфера
                addressCounter -= (cur_packet_size - 3);
                if (addressCounter < 0) addressCounter += maxLenBuffer;    // дошли до конца кольцевого буфера
                if ((InpDataCurType == PacketType.PACKET_TYPE_MONITORING_DATA)
                    || (InpDataCurType == PacketType.PACKET_TYPE_ECG_PACE_DATA)
                    || (InpDataCurType == PacketType.PACKET_TYPE_PACE_DATA)
                    || (InpDataCurType == PacketType.PACKET_TYPE_BR_DATA)
                    || (InpDataCurType == PacketType.PACKET_TYPE_BATT_DATA)
                    || (InpDataCurType == PacketType.PACKET_TYPE_SING_PACE)
                    || (InpDataCurType == PacketType.PACKET_TYPE_PACE_PKT))
                {
                    switch (InpDataCurType)
                    {
                        case PacketType.PACKET_TYPE_MONITORING_DATA:    // пакет значений АЦП ЭКГ
                            ECGDataType();
                            break;
                        case PacketType.PACKET_TYPE_PACE_DATA:	// пакет pacemaker
                            PaceDataType();
                            break;
                        case PacketType.PACKET_TYPE_BATT_DATA:  // пакет о состоянии батарейки
                            BattDataType();
                            break;
                        case PacketType.PACKET_TYPE_BR_DATA:    // пакет о канале дыхания
                            BRDataType();
                            break;
                    }
                    LocBufferLen -= cur_packet_size;
                    int k = LocBufferPosition;
                    for (i = 0; i < cur_packet_size; i++)
                    {
                        if ((k + i) >= maxLenBuffer)
                        {
                            k -= maxLenBuffer;
                        }
                        ringbuf[k + i] = 0x00;   // обнуляем принятый буфер
                    }
                }
            } while (LocBufferLen > cur_packet_size);
        finished: ;
        }

        private static byte ReadByteRingBuffer()
        {
            if (addressCounter >= maxLenBuffer)
            {
                addressCounter = 0;
            }
            if (addressCounter >= 0)
            {
                return ringbuf[addressCounter++];
            }
            else return 0;
        }

        #region основой протокол
        /// <summary>
        /// Читаем данные ЭКГ
        /// </summary>
        static void ECGDataType()
        {
            byte[] MaskLeadOff = { 0, 0 };  // маска обрывов
            int CurReadingFrameIndexECG = 0;   // индекс фреймов (пакетов) ЭКГ
            int channelIndex;
            int BrValue = 0;     // показания канала дыхания
            int indexBr = 0;
            int channels_size = 0;   // размер разницы каналов
            Int32 CurReadingValue = 0; // текущее абсоютное значение ЭКГ

            // читаем 1-й байт маски обрывов
            MaskLeadOff[1] = ReadByteRingBuffer();
            // читаем 2-й байт маски обрывов
            MaskLeadOff[0] = ReadByteRingBuffer();
            // читаем номер первого отсчета - 4 байта
            NumFirstADCFrame = ReadByteRingBuffer();
            NumFirstADCFrame |= ReadByteRingBuffer() << 8;
            NumFirstADCFrame |= ReadByteRingBuffer() << 16;
            NumFirstADCFrame |= ReadByteRingBuffer() << 24;
            // читаем кол-во отсчетов в пакете    
            NumFramesInCurPacket = ReadByteRingBuffer();
            if ((InpDataCurType == PacketType.PACKET_TYPE_ECG_PACE_DATA) && (counter_ecg_pace == 0))
            {
                indication_pace = ReadByteRingBuffer(); // читаем первый признак pacemaker за следующие 8 отсчётов
                counter_ecg_pace++; // увеличиваем счётчик для признака PACE
            }
            CurReadingFrameIndexECG = 0;
            // читаем 1-й фрейм ацп - абсолютные значения
            for (channelIndex = 0; channelIndex < myChannelsCount - 1; channelIndex++)
            {
                MonitoringData.channelData[channelIndex] = new float[NumFramesInCurPacket];
                CurReadingValue = 0;
                CurReadingValue |= ReadByteRingBuffer();
                CurReadingValue |= ReadByteRingBuffer() << 8;
                CurReadingValue |= ReadByteRingBuffer() << 16;
                if (Convert.ToBoolean(CurReadingValue & (1 << 23))) CurReadingValue |= 0xFF << 24;
                MonitoringData.channelData[channelIndex][CurReadingFrameIndexECG] = CurReadingValue;
            }
            MonitoringData.channelData[channelIndex] = new float[NumFramesInCurPacket];
            counter_breath++;
            if (counter_breath >= 20)
            {
                counter_breath = 0;
                BrValue = ReadByteRingBuffer();
                BrValue |= ReadByteRingBuffer() << 8;
                if (MonitoringData.onOfBr) MonitoringData.channelData[channelIndex][indexBr++] = BrValue;
                else MonitoringData.channelData[channelIndex][indexBr++] = 0.0f;
            }
            CurReadingFrameIndexECG++; // переходим к чтению остальных фреймов
            while (CurReadingFrameIndexECG < NumFramesInCurPacket)
            {
                if (InpDataCurType == PacketType.PACKET_TYPE_ECG_PACE_DATA)
                {
                    if (counter_ecg_pace == 0)
                    {
                        indication_pace = ReadByteRingBuffer(); // читаем признак pacemaker за 8 отсчётов
                    }
                    counter_ecg_pace++; // увеличиваем счётчик для признака PACE
                    if (counter_ecg_pace >= 8) counter_ecg_pace = 0;
                }
                // читаем размер разниц каналов
                channels_size = ReadByteRingBuffer();
                channels_size |= ReadByteRingBuffer() << 8;
                for (channelIndex = 0; channelIndex < myChannelsCount - 1; channelIndex++)
                {
                    // переходим к разбору разниц
                    CurReadingValue = 0;
                    if ((channels_size & 0xC000) == 0x4000)
                    {
                        CurReadingValue |= ReadByteRingBuffer();
                        if (Convert.ToBoolean(CurReadingValue & (1 << 7))) CurReadingValue |= 0xFFFFFF << 8;
                        MonitoringData.channelData[channelIndex][CurReadingFrameIndexECG] = MonitoringData.channelData[channelIndex][CurReadingFrameIndexECG - 1] + CurReadingValue;
                    }
                    else
                    {
                        if ((channels_size & 0xC000) == 0x8000)
                        {
                            // 2 байта разницы
                            CurReadingValue |= ReadByteRingBuffer();
                            CurReadingValue |= ReadByteRingBuffer() << 8;
                            if (Convert.ToBoolean(CurReadingValue & (1 << 15))) CurReadingValue |= 0xFFFF << 16;
                            MonitoringData.channelData[channelIndex][CurReadingFrameIndexECG] = MonitoringData.channelData[channelIndex][CurReadingFrameIndexECG - 1] + CurReadingValue;
                        }
                        else
                        {
                            // 3 байта - абсолютное значение 
                            CurReadingValue |= ReadByteRingBuffer();
                            CurReadingValue |= ReadByteRingBuffer() << 8;
                            CurReadingValue |= ReadByteRingBuffer() << 16;
                            if (Convert.ToBoolean(CurReadingValue & (1 << 23))) CurReadingValue |= 0xFF << 24;
                            MonitoringData.channelData[channelIndex][CurReadingFrameIndexECG] = CurReadingValue;
                        }
                    }
                    channels_size <<= 2;
                }
                counter_breath++;
                if (counter_breath >= 20)   // читаем канал дыхания
                {
                    counter_breath = 0;
                    BrValue = ReadByteRingBuffer();
                    BrValue |= ReadByteRingBuffer() << 8;
                    if (MonitoringData.onOfBr) MonitoringData.channelData[channelIndex][indexBr++] = BrValue;
                    else MonitoringData.channelData[channelIndex][indexBr++] = 0.0f;
                }
                CurReadingFrameIndexECG++;
            }
            // дочитываем последние два байта пакета - crc (контрольная сумма)
            ushort crc = ReadByteRingBuffer();
            crc |= (ushort)(ReadByteRingBuffer() << 8);
            PreparationDataForView(NumFramesInCurPacket);
            NormalizationData(NumFramesInCurPacket);
            DoFilterBr();
            // формируем массив состояний (данных) первого отсчета каждого канала для следующего отсчета
            for (channelIndex = 0; channelIndex < myChannelsCount - 1; channelIndex++)
            {
                MonitoringData.firstSampleState[channelIndex] = MonitoringData.channelData[channelIndex][NumFramesInCurPacket - 1];
//                firstDataECG = true;
            }
            // дыхание
            MonitoringData.firstSampleState[channelIndex] = MonitoringData.channelData[channelIndex][indexBr - 1];
        }
        #endregion

        /// <summary>
        /// Конфигурация фильтра канала дыхания
        /// </summary>
        public static void FilterBr()
        {
            Filter[] _filters = new Filter[1];
            double[][] coeff;
            coeff = ButterworthIIRF_Designing.GetCoefficients_1_Standard(
                FilterType.Band, 4, 1, MonitoringData.DataRate / MonitoringData.FreqDividerBr, 0.01, 10);
            _filters[0] = new IIRFilter(coeff[0], coeff[1]);
            _filters[0].Active = true;
            filtersBr = _filters;
        }

        /// <summary>
        /// Выполняем фильтрацию канала дыхания
        /// </summary>
        private static void DoFilterBr()
        {
            float[] dataBr = MonitoringData.channelData[8];
            Filter filterBr = filtersBr[0];
            if (filterBr != null)
                filterBr.DoFilter(dataBr, dataBr, 0, 0, 2);
        }

        /// <summary>
        /// подготавливаем данные для вывода -
        /// распределяем данные по каналам отображения
        /// </summary>
        private static void PreparationDataForView(byte numFramesInCurPacket)
        {
            for (int k = 0; k < numFramesInCurPacket; k++)
            {
                float temp = MonitoringData.channelData[5][k];
                MonitoringData.channelData[5][k] = MonitoringData.channelData[0][k];
                MonitoringData.channelData[0][k] = temp;   // L
                temp = MonitoringData.channelData[3][k];
                MonitoringData.channelData[3][k] = MonitoringData.channelData[1][k];
                MonitoringData.channelData[1][k] = temp;   // F
                temp = MonitoringData.channelData[3][k];
                MonitoringData.channelData[3][k] = MonitoringData.channelData[2][k];
                MonitoringData.channelData[2][k] = temp;   // C1
                temp = MonitoringData.channelData[7][k];
                MonitoringData.channelData[7][k] = MonitoringData.channelData[3][k];
                MonitoringData.channelData[3][k] = temp;   // C2
                temp = MonitoringData.channelData[6][k];
                MonitoringData.channelData[6][k] = MonitoringData.channelData[4][k];
                MonitoringData.channelData[4][k] = temp;   // C3
                temp = MonitoringData.channelData[6][k];
                MonitoringData.channelData[6][k] = MonitoringData.channelData[5][k];
                MonitoringData.channelData[5][k] = temp;   // C4
                temp = MonitoringData.channelData[7][k];
                MonitoringData.channelData[7][k] = MonitoringData.channelData[6][k];
                MonitoringData.channelData[6][k] = temp;   // C5, C6
            }
        }

        /// <summary>
        /// Нормализация параметров
        /// </summary>
        private static void NormalizationData(byte numFramesInCurPacket)
        {
            int channelIndex;
            for (int k = 0; k < numFramesInCurPacket; k++)
            {
                for (channelIndex = 0; channelIndex < myChannelsCount - 1; channelIndex++)
                {
                    // нормализуем параметры АЦП ADS1298
                    MonitoringData.channelData[channelIndex][k] *= (4.0f / (0x007fffff * 6.0f));
                }
                // нормализуем параметр канала дыхания
                MonitoringData.channelData[channelIndex][k] *= ((3.3f * 100) / (0x00000fff * 45.45f)) / 10;
            }
        }

        /// <summary>
        /// Читаем данные pacemaker
        /// </summary>
        private static void PaceDataType()
        {
        }

        /// <summary>
        /// Читаем данные о батарейке
        /// </summary>
        private static void BattDataType()
        {
        }

        /// <summary>
        /// Читаем данные с канала дыхания
        /// </summary>
        private static void BRDataType()
        {
        }

        static class PacketType
        {
            /// <summary>
            /// признак начала пакета
            /// </summary>
            public const byte BLUETOOTH_PACKET_BEGIN_MARKER = 0xC0;
            /// <summary>
            /// тип пакета ЭКГ + признак PACE
            /// </summary>
            public const byte PACKET_TYPE_ECG_PACE_DATA = 0x23;
            /// <summary>
            /// тип пакета АЦП (ЭКГ) + канал дыхания
            /// </summary>
            public const byte PACKET_TYPE_MONITORING_DATA = 0x24;
            public const byte PACKET_TYPE_PACE_DATA = 0x25;	// тип пакета данных кардиостимулятора (PACE)
            /// <summary>
            /// тип пакета данных о батарейке
            /// </summary>
            public const byte PACKET_TYPE_BATT_DATA = 0x26;
            public const byte PACKET_TYPE_BR_DATA = 0x27;	// тип пакета канала дыхания
            public const byte PACKET_TYPE_SING_PACE = 0x28;	// тип пакета с признаками pacemaker
            /// <summary>
            /// тип пакета с пакетными данными pacemaker
            /// </summary>
            public const byte PACKET_TYPE_PACE_PKT = 0x29;
        }
        #endregion

    }
}
