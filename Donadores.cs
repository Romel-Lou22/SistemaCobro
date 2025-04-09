using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace SistemaCobro
{
    public partial class Donadores : Form
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["miConexion"].ConnectionString;
        int rowsAffected;
        private bool donacionInsertada = false;
        public Donadores()
        {
            InitializeComponent();
        }

        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            donacionInsertada = false;
            // Get values from TextBoxes
            string codigo = txtCodigo.Text;
            string cedula = txtCedula.Text;
            string nombre = txtNombres.Text;
            string cantidad = txtCantidad.Text;  // Fixed field name for 'cantidad'
            DateTime fechaP = DateTime.Now;
            MessageBox.Show(fechaP.ToString(), "Fecha/Hora actual");
            // Validation for empty fields
            if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(cedula) || string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(cantidad))
            {
                MessageBox.Show("Todos los campos son obligatorios.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate that 'codigo' and 'cedula' contain only numbers
            if (!Regex.IsMatch(codigo, @"^\d+$"))
            {
                MessageBox.Show("El código debe contener solo números.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Regex.IsMatch(cedula, @"^\d+$"))
            {
                MessageBox.Show("La cédula debe contener solo números.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate that 'cantidad' is a valid decimal number
            if (!decimal.TryParse(cantidad, out decimal cantidadDecimal))
            {
                MessageBox.Show("La cantidad debe ser un valor numérico.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // SQL query to insert data
            string query = "INSERT INTO Donadores(codigo, cedula, nombre, cantidad, fecha) VALUES(@codigo, @cedula, @nombre, @cantidad, @fechaP)";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Assign parameters
                        cmd.Parameters.AddWithValue("@codigo", codigo);
                        cmd.Parameters.AddWithValue("@cedula", cedula);
                        cmd.Parameters.AddWithValue("@nombre", nombre);
                        cmd.Parameters.AddWithValue("@cantidad", cantidadDecimal);  // Ensure 'cantidad' is decimal
                        cmd.Parameters.AddWithValue("@fechaP",fechaP);

                        rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Datos insertados correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            donacionInsertada = true;
                        }
                        else
                        {
                            MessageBox.Show("Error al insertar los datos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            donacionInsertada = false;
                        }
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Error SQL al insertar el donador: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    donacionInsertada = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al insertar datos en la tabla: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    donacionInsertada = false;
                }
            }
        }

        private void btnImprimir_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCodigo.Text.Trim()) || string.IsNullOrEmpty(txtCedula.Text.Trim()))
            {
                MessageBox.Show("Llene todos los campos", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (VerificarDonacionInsertada(txtCodigo.Text.Trim(), txtCedula.Text.Trim()))
            {
                ImprimirComprobante();
            }
            else
            {
                MessageBox.Show("Registre su donación antes de imprimir", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void ImprimirComprobante()
        {
            // Recoger los datos ingresados
            string codigo = txtCodigo.Text;
            string cedula = txtCedula.Text;
            string nombre = txtNombres.Text;
            string cantidad = txtCantidad.Text;
            DateTime fechaP = DateTime.Now;

            // Obtener la ruta de la carpeta "Documentos" del usuario
            string rutaDocumentos = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            // Especificar la ruta donde se guardarán los comprobantes, dentro de "Documentos"
            string rutaBase = Path.Combine(rutaDocumentos, "Comprobantes");

            // Asegurarse de que la carpeta exista
            Directory.CreateDirectory(rutaBase);

            // Crear el nombre del archivo PDF
            string nombreArchivo = $"Comprobante_{cedula}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            string rutaCompleta = Path.Combine(rutaBase, nombreArchivo);

            try
            {
                // Tamaño del papel personalizado
                Rectangle tamañoPapel = new Rectangle(288f, 500f);

                // Crear el documento PDF con el tamaño especificado
                Document doc = new Document(tamañoPapel, 20f, 20f, 30f, 30f); // Aumentamos los márgenes
                PdfWriter.GetInstance(doc, new FileStream(rutaCompleta, FileMode.Create));

                // Abrir el documento
                doc.Open();

                // Crear una fuente profesional para recibos, estilo "Courier"
                Font fuenteTitulo = new Font(iTextSharp.text.Font.FontFamily.COURIER, 12f, iTextSharp.text.Font.BOLD);  // Fuente para títulos
                Font fuenteContenido = new Font(iTextSharp.text.Font.FontFamily.COURIER, 10f, iTextSharp.text.Font.NORMAL);  // Fuente para contenido

                // Agregar el encabezado
                Paragraph encabezado = new Paragraph("COMITÉ BARRIAL PILACOTO\nCOMISIÓN CONSTRUCCIÓN IGLESIA\nR.U.C: 000000000000\nPROVINCIA: COTOPAXI CANTÓN: LATACUNGA\nCIUDAD: PILACOTO COMUNA: PILACOTO\nDIRECCIÓN: PILACOTO\nTELÉFONO: 03-0000000\n\n", fuenteTitulo);
                encabezado.Alignment = Element.ALIGN_CENTER;
                doc.Add(encabezado);
                // SUBTITULO
                Paragraph comp = new Paragraph("COMPROBANTE DE DONACIÓN", fuenteContenido);
                comp.Alignment = Element.ALIGN_CENTER;
                doc.Add(comp);
                // Agregar una línea divisoria
                doc.Add(new Paragraph("----------------------------------------------------------------------------------", fuenteContenido));

                // Agregar los datos ingresados
                doc.Add(new Paragraph($"Código: {codigo}", fuenteContenido));
                doc.Add(new Paragraph($"Cédula: {cedula}", fuenteContenido));
                doc.Add(new Paragraph($"Nombres: {nombre}", fuenteContenido));
                doc.Add(new Paragraph($"FechaPago:{fechaP}",fuenteContenido));
    

                doc.Add(new Paragraph($"Cantidad: ${cantidad:F2}", fuenteContenido));

                // Agregar otra línea divisoria
                doc.Add(new Paragraph("----------------------------------------------------------------------------------", fuenteContenido));

                // Mensaje de agradecimiento
                Paragraph gracias = new Paragraph("GRACIAS POR SU DONACIÓN", fuenteContenido
                    
                    );
                gracias.Alignment = Element.ALIGN_CENTER;
                doc.Add(gracias);

                // Cerrar el documento
                doc.Close();

                MessageBox.Show("El PDF ha sido generado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Abrir el archivo PDF automáticamente
                System.Diagnostics.Process.Start(rutaCompleta);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        //VALIDACIÓN  PARA VERIFICAR
        private bool VerificarDonacionInsertada(string codigo, string cedula)
        {
            bool donacionRegistrada = false;
            string queryVerificar = "SELECT COUNT(*) FROM Donadores WHERE codigo = @codigo AND cedula = @cedula";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(queryVerificar, conn))
                    {
                        cmd.Parameters.AddWithValue("@codigo", codigo);
                        cmd.Parameters.AddWithValue("@cedula", cedula);

                        int count = (int)cmd.ExecuteScalar();
                        donacionRegistrada = (count > 0);
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Error SQL al verificar la donación: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al verificar la donación: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return donacionRegistrada;
        }

       /* private void btnImprimir_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCodigo.Text.Trim()) || string.IsNullOrEmpty(txtCedula.Text.Trim()))
            {
                MessageBox.Show("Llene todos los campos", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (VerificarDonacionInsertada(txtCodigo.Text.Trim(), txtCedula.Text.Trim()))
            {
                ImprimirComprobante();
            }
            else
            {
                MessageBox.Show("Registre su donación antes de imprimir", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
       */



    }
}


