using System;
using System.Collections.Generic;

namespace FileSizeEnumerator.UI
{
    public class SizeControlViewModel : ModelBase
    {
        #region Fields

        private SizeUnits selectedUnit;
        private double size;

        #endregion

        public IEnumerable<string> Units => Enum.GetNames(typeof(SizeUnits));

        public SizeUnits SelectedUnit
        {
            get => this.selectedUnit;
            set => SetProperties(ref this.selectedUnit, value, nameof(SelectedUnit), nameof(Length));
        }

        public ulong Length
        {
            get => (ulong)Math.Max(0, Size * GetUnitQuantity(SelectedUnit));
            set
            {
                if (size < 1000)
                {
                    size = Math.Max(0, size);
                    selectedUnit = SizeUnits.B;
                }
                else if (size >= 1000 && size < 1_000_000)
                {
                    size = Math.Round(size / 1000, 2);
                    selectedUnit = SizeUnits.kB;
                }
                else if (size >= 1_000_000 && size < 1_000_000_000)
                {
                    size = Math.Round(size / 1_000_000, 2);
                    selectedUnit = SizeUnits.MB;
                }
                else if (size >= 1_000_000_000 && size < 1_000_000_000_000)
                {
                    size = Math.Round(size / 1_000_000_000, 2);
                    selectedUnit = SizeUnits.GB;
                }
                else
                {
                    size = Math.Round(size / 1_000_000_000_000, 2);
                    selectedUnit = SizeUnits.TB;
                }
                RaisePropertyChanged(nameof(Size));
                RaisePropertyChanged(nameof(SelectedUnit));
            }
        }

        public double Size
        {
            get => this.size;
            set => SetProperties(ref this.size, value, nameof(Size), nameof(Length));
        }

        private double GetUnitQuantity(SizeUnits sizeUnits) =>
            sizeUnits switch
            {
                SizeUnits.kB => 1000.0,
                SizeUnits.MB => 1_000_000.0,
                SizeUnits.GB => 1_000_000_000.0,
                SizeUnits.TB => 1_000_000_000_000.0,
                _ => 1.0,
            };
    }
}
