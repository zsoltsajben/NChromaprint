using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace NChromaprint.Classes
{
    public class Image
    {
        public int NeededNumCols { get; set; }

        DenseMatrix Data { get; set; }
        public int NumCols
        {
            get
            {
                if (Data != null)
                    return Data.ColumnCount;
                return 0;
            }
        }
        public int NumRows
        {
            get
            {
                if (Data != null)
                    return Data.RowCount;
                return 0;
            }
        }


        public Image(int columns)
        {
            NeededNumCols = columns;
            Data = null;
        }

        public Image(int columns, int rows)
        {
            NeededNumCols = columns;
            Data = new DenseMatrix(rows, columns);
        }


        public void AddRow(Vector<double> row)
        {
            if (Data == null)
            {
                Data = new DenseMatrix(1, NeededNumCols, row.ToArray());
            }
            else
            {
                Data = (DenseMatrix)Data.InsertRow(Data.RowCount, row);
            }
        }

        public double this[int r, int c]
        {
            get
            {
                return this[r][c];
            }
            set
            {
                var temp = this[r];
                temp[c] = value;
                this[r] = temp;
            }
        }

        public Vector<double> this[int i]
        {
            get
            {
                return GetRow(i);
            }
            set
            {
                SetRow(i, value);
            }
        }

        public Vector<double> GetRow(int i)
        {
            return Data.Row(i);
        }

        public void SetRow(int i, Vector<double> row)
        {
            Data.SetRow(i, row);
        }
    }
}
