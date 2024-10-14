using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace SistemaCobro
{
    public partial class Reportes : Form
    {
        // Cadena de conexión a tu base de datos
        private string connectionString = ConfigurationManager.ConnectionStrings["miConexion"].ConnectionString;
        public Reportes()
        {
            InitializeComponent();
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBuscar.Text))
            {
                MessageBox.Show("Por favor, ingrese un criterio de búsqueda.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string query = @"SELECT CodigoUsuario, Cedula, UsuarioSistema, FechaPago, MontoPago, TipoPago 
                     FROM Pagos 
                     WHERE (Cedula = @cedula OR CodigoUsuario = @codigo OR UsuarioSistema LIKE @nombres) 
                     AND FechaPago BETWEEN @fechaInicio AND @fechaFin";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@cedula", txtBuscar.Text);
                        cmd.Parameters.AddWithValue("@codigo", txtBuscar.Text);
                        cmd.Parameters.AddWithValue("@nombres", "%" + txtBuscar.Text + "%");
                        cmd.Parameters.AddWithValue("@fechaInicio", dtpFechaInicio.Value.Date);
                        cmd.Parameters.AddWithValue("@fechaFin", dtpFechaFinal.Value.Date.AddDays(1).AddSeconds(-1));

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                dgvPagos.DataSource = dt;
                                FormatearDataGridView();
                            }
                            else
                            {
                                MessageBox.Show("No se encontraron resultados.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                dgvPagos.DataSource = null;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void FormatearDataGridView()
        {
            dgvPagos.Columns["MontoPago"].DefaultCellStyle.Format = "N2";
            dgvPagos.Columns["FechaPago"].DefaultCellStyle.Format = "dd/MM/yyyy";
            // Ajusta el ancho de las columnas según sea necesario
            dgvPagos.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        private void btnImprimir_Click(object sender, EventArgs e)
        {
            if (ValidarD())
            {
                string cedula = txtBuscar.Text;

                // Obtener la ruta de la carpeta "Documentos" del usuario
                string rutaDocumentos = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                // Especificar la ruta donde se guardarán los comprobantes, dentro de "Documentos"
                string rutaBase = Path.Combine(rutaDocumentos, "Comprobantes");

                // Asegurarse de que la carpeta exista, si no, crearla
                Directory.CreateDirectory(rutaBase);

                // Crear el nombre del archivo PDF
                string nombreArchivo = $"Comprobante_{cedula}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string rutaCompleta = Path.Combine(rutaBase, nombreArchivo);

                try
                {
                    iTextSharp.text.Rectangle tamañoPapel = new iTextSharp.text.Rectangle(288f, 600f); // 288 puntos = 4 pulgadas, altura arbitrariamente grande

                    // Crear el documento PDF con el tamaño personalizado
                    Document documento = new Document(tamañoPapel, 10f, 10f, 10f, 10f); // Márgenes reducidos
                    PdfWriter writer = PdfWriter.GetInstance(documento, new FileStream(rutaCompleta, FileMode.Create));

                    // Abrir el documento para agregar contenido
                    documento.Open();

                    // Estilo de fuente para el texto usando Courier
                    iTextSharp.text.Font fuente = FontFactory.GetFont(FontFactory.COURIER, 10, iTextSharp.text.Font.BOLDITALIC);


                    // Título del comprobante
                    Paragraph encabezado = new Paragraph("COMITÉ BARRIAL PILACOTO\nCOMISIÓN CONSTRUCCIÓN IGLESIA\nR.U.C: 000000000000\nPROVINCIA: COTOPAXI CANTÓN: LATACUNGA\nCIUDAD: PILACOTO COMUNA: PILACOTO\nDIRECCIÓN: PILACOTO\nTELÉFONO: 03-0000000\n\n", fuente);
                    encabezado.Alignment = Element.ALIGN_CENTER;
                    documento.Add(encabezado);

                    // Espacio entre el encabezado y el contenido
                    documento.Add(new Paragraph("\n"));

                    // Crear una tabla de 2 columnas
                    PdfPTable tabla = new PdfPTable(2);
                    tabla.WidthPercentage = 100; // Ocupar el 100% del ancho de la página

                    // Agregar celdas con los nombres de los encabezados de las columnas
                    PdfPCell celda1 = new PdfPCell(new Phrase("Descripción", fuente));
                    PdfPCell celda2 = new PdfPCell(new Phrase("Valor", fuente));

                    // Alineación del texto en las celdas
                    celda1.HorizontalAlignment = Element.ALIGN_CENTER;
                    celda2.HorizontalAlignment = Element.ALIGN_CENTER;

                    // Color de fondo de las celdas de encabezado (opcional)
                    celda1.BackgroundColor = new BaseColor(200, 200, 200);
                    celda2.BackgroundColor = new BaseColor(200, 200, 200);

                    // Agregar las celdas a la tabla
                    tabla.AddCell(celda1);
                    tabla.AddCell(celda2);

                    // Aquí puedes agregar los resultados de la búsqueda del reporte
                    // Ejemplo de datos de pago
                    foreach (DataGridViewRow fila in dgvPagos.Rows)
                    {
                        if (fila.Cells[0].Value != null) // Asegurarse de que la fila no esté vacía
                        {
                            tabla.AddCell(new Phrase("Código de Usuario", fuente));
                            tabla.AddCell(new Phrase(fila.Cells["CodigoUsuario"].Value.ToString(), fuente));

                            tabla.AddCell(new Phrase("Nombre del Usuario", fuente));
                            tabla.AddCell(new Phrase(fila.Cells["UsuarioSistema"].Value.ToString(), fuente));

                            tabla.AddCell(new Phrase("Fecha de Pago", fuente));
                            // tabla.AddCell(new Phrase(fila.Cells["FechaPago"].Value.ToString(), fuente));
                            tabla.AddCell(new Phrase(Convert.ToDateTime(fila.Cells["FechaPago"].Value).ToString("dd/MM/yyyy"), fuente));


                            tabla.AddCell(new Phrase("Monto de Pago", fuente));
                            tabla.AddCell(new Phrase(fila.Cells["MontoPago"].Value.ToString(), fuente));

                            tabla.AddCell(new Phrase("Tipo de Pago", fuente));
                            tabla.AddCell(new Phrase(fila.Cells["TipoPago"].Value.ToString(), fuente));

                            // Agregar fila vacía
                            tabla.AddCell(new Phrase(" ", fuente));
                            tabla.AddCell(new Phrase(" ", fuente));
                        }
                    }

                    // Agregar la tabla al documento
                    documento.Add(tabla);

                    // Cerrar el documento
                    documento.Close();

                    MessageBox.Show("Comprobante generado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Abrir el PDF generado
                    System.Diagnostics.Process.Start(rutaCompleta);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al generar el comprobante: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void Reportes_Load(object sender, EventArgs e)
        {

        }

        private bool ValidarD()
        {
            if (string.IsNullOrEmpty(txtBuscar.Text.Trim()))
            {
                MessageBox.Show("Ingrese criterio de busqueda", "Importante", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }
    }
}
