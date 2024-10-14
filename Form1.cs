using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Configuration;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace SistemaCobro
{
    public partial class Form1 : Form
    {
        // Cadena de conexión a tu base de datos
        private string connectionString = ConfigurationManager.ConnectionStrings["miConexion"].ConnectionString;
        private AutoCompleteStringCollection autoCompleteCollection;
        int rowsAffected;




        public Form1()
        {
            InitializeComponent();
            ConfigureAutocomplete();

        }

        private void ConfigureAutocomplete()
        {
            autoCompleteCollection = new AutoCompleteStringCollection();
            txtBuscar.AutoCompleteMode = AutoCompleteMode.Suggest;
            txtBuscar.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtBuscar.AutoCompleteCustomSource = autoCompleteCollection;
            LoadAutoCompleteData();
        }

        // Evento al cargar el formulario
        private void Form1_Load(object sender, EventArgs e)
        {
            // Configurar el texto del ToolTip para txtBuscar
           toolTip.SetToolTip(txtBuscar, "Ingrese cedula, código o nombres completos para buscar.");

            // Opcional: Configurar el tiempo que el tooltip permanecerá visible
            toolTip.AutoPopDelay = 5000;  // Tiempo en milisegundos (5 segundos)
            toolTip.InitialDelay = 100;   // Retraso inicial en milisegundos
            toolTip.ReshowDelay = 500;    // Tiempo entre mostrar el tooltip nuevamente
            toolTip.ShowAlways = true;

            

        }

        // Método para probar la conexión a la base de datos
        private void TestConnection()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open(); // Abre la conexión
                    MessageBox.Show("Conexión exitosa a la base de datos.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (SqlException ex)
            {
                // Manejo de errores SQL
                MessageBox.Show($"Error SQL al conectar a la base de datos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // Manejo de errores generales
                MessageBox.Show($"Error al conectar a la base de datos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            TestConnection();
        }

        //BTNBUSCAR
        private void btnBuscar_Click_1(object sender, EventArgs e)
        {
            if (ValidarEntrada())
            {
                string query = "";

                // Check if the search text is a number (to search by CodigoUsuario or Cedula)
                bool isNumeric = long.TryParse(txtBuscar.Text, out _);

                // Modify the query based on the input type
                if (isNumeric)
                {
                    query = "SELECT CodigoUsuario, Cedula, UsuarioSistema FROM Usuarios WHERE Cedula = @cedula OR CodigoUsuario = @codigo";
                }
                else
                {
                    query = "SELECT CodigoUsuario, Cedula, UsuarioSistema FROM Usuarios WHERE UsuarioSistema LIKE @nombres";
                }

                // Connect to the database
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand(query, conn);

                        if (isNumeric)
                        {
                            // Set parameters for numeric search
                            cmd.Parameters.Add(new SqlParameter("@cedula", System.Data.SqlDbType.VarChar) { Value = txtBuscar.Text });
                            cmd.Parameters.Add(new SqlParameter("@codigo", System.Data.SqlDbType.BigInt) { Value = Convert.ToInt64(txtBuscar.Text) });
                        }
                        else
                        {
                            // Set parameters for name search
                            cmd.Parameters.Add(new SqlParameter("@nombres", System.Data.SqlDbType.NVarChar) { Value = "%" + txtBuscar.Text + "%" });
                        }

                        SqlDataReader reader = cmd.ExecuteReader();

                        // Check if any results were found
                        if (reader.Read())
                        {
                            txtCodigo.Text = reader["CodigoUsuario"].ToString();
                            txtCedula.Text = reader["Cedula"].ToString();
                            txtNombres.Text = reader["UsuarioSistema"].ToString();
                        }
                        else
                        {
                            MessageBox.Show("No se encontraron resultados.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show("Error SQL: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

        }

        private void btnCobrar_Click(object sender, EventArgs e)
        {
            // Verificar el tipo de pago y asignar el monto correspondiente
            decimal montoPago = 0;
            string tipoPago = "";

            if (RbMensual.Checked)
            {
                montoPago = 10;
                tipoPago = "Mensual";
            }
            else if (RbAnual.Checked)
            {
                montoPago = 120;
                tipoPago = "Anual";
            }
            else
            {
                MessageBox.Show("Por favor, seleccione un tipo de pago.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Salir del método si no se seleccionó un tipo de pago
            }

            // Obtener los valores de los TextBox
            string codigoUsuario = txtCodigo.Text;
            string cedula = txtCedula.Text;
            string nombres = txtNombres.Text;
            DateTime fechaPago = Date.Value; // Usar la fecha seleccionada en el DateTimePicker

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open(); // Abrir la conexión

                    // Nueva validación para evitar pago anual si ya hay un pago en el mismo mes
                    string validationQueryMesActual = @"SELECT COUNT(*) FROM Pagos 
                                                WHERE CodigoUsuario = @codigoUsuario 
                                                AND MONTH(FechaPago) = MONTH(@fechaPago) 
                                                AND YEAR(FechaPago) = YEAR(@fechaPago)";

                    using (SqlCommand validationCmdMesActual = new SqlCommand(validationQueryMesActual, conn))
                    {
                        validationCmdMesActual.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);
                        validationCmdMesActual.Parameters.AddWithValue("@fechaPago", fechaPago);

                        int existingPaymentsInMonth = (int)validationCmdMesActual.ExecuteScalar();

                        if (existingPaymentsInMonth > 0)
                        {
                            MessageBox.Show("Ya tiene un pago registrado este mes. No se puede realizar otro pago, sea mensual o anual, hasta el próximo mes.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return; // Salir del método si ya existe un pago en el mes actual
                        }
                    }

                    // Validar si ya existe un pago según el tipo
                    string validationQuery = "";
                    if (tipoPago == "Mensual")
                    {
                        validationQuery = @"SELECT COUNT(*) FROM Pagos 
                                    WHERE CodigoUsuario = @codigoUsuario 
                                    AND TipoPago = 'Anual'
                                    AND YEAR(FechaPago) = YEAR(@fechaPago)";

                        using (SqlCommand validationCmd = new SqlCommand(validationQuery, conn))
                        {
                            validationCmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);
                            validationCmd.Parameters.AddWithValue("@fechaPago", fechaPago);

                            int existingAnnualPayments = (int)validationCmd.ExecuteScalar();

                            if (existingAnnualPayments > 0)
                            {
                                MessageBox.Show("Ya tiene un pago anual registrado este año, no puede realizar un pago mensual.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return; // Salir del método si ya existe un pago anual en el año
                            }
                        }
                    }
                    else // Anual
                    {
                        validationQuery = @"SELECT COUNT(*) FROM Pagos 
                                    WHERE CodigoUsuario = @codigoUsuario 
                                    AND TipoPago = 'Anual'
                                    AND FechaPago >= DATEADD(YEAR, -1, @fechaPago)";
                    }

                    using (SqlCommand validationCmd = new SqlCommand(validationQuery, conn))
                    {
                        validationCmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);
                        validationCmd.Parameters.AddWithValue("@fechaPago", fechaPago);

                        int existingPayments = (int)validationCmd.ExecuteScalar();

                        if (existingPayments > 0)
                        {
                            if (tipoPago == "Mensual")
                            {
                                MessageBox.Show("Ya tiene un pago realizado en este mes.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            else
                            {
                                MessageBox.Show("Ya tiene un pago anual vigente.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            return; // Salir del método si ya existe un pago
                        }
                    }

                    // Si no hay pagos existentes, proceder con la inserción
                    string insertQuery = @"INSERT INTO Pagos (CodigoUsuario, Cedula, UsuarioSistema, FechaPago, MontoPago, TipoPago) 
                                   VALUES (@codigoUsuario, @cedula, @nombres, @fechaPago, @montoPago, @tipoPago)";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);
                        cmd.Parameters.AddWithValue("@cedula", cedula);
                        cmd.Parameters.AddWithValue("@nombres", nombres);
                        cmd.Parameters.AddWithValue("@fechaPago", fechaPago);
                        cmd.Parameters.AddWithValue("@montoPago", montoPago);
                        cmd.Parameters.AddWithValue("@tipoPago", tipoPago);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Pago registrado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("No se pudo registrar el pago. Intente nuevamente.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Error SQL: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void btnComprobante_Click(object sender, EventArgs e)
        {
            //  COMPROBANTE VALIDACIONES
            if (rowsAffected > 0)
            {
                if (RbMensual.Checked || RbAnual.Checked)
                {
                    // Obtener los valores necesarios para generar el comprobante
                    string codigoUsuario = txtCodigo.Text;
                    string cedula = txtCedula.Text;
                    string nombres = txtNombres.Text;
                    DateTime fechaPago = Date.Value;
                    decimal montoPago = RbMensual.Checked ? 10 : 120;
                    string tipoPago = RbMensual.Checked ? "Mensual" : "Anual";

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
                        Rectangle tamañoPapel = new Rectangle(288f, 500f); // 288 puntos = 4 pulgadas, altura arbitrariamente grande

                        // Crear el documento PDF con el tamaño personalizado
                        Document documento = new Document(tamañoPapel, 10f, 10f, 10f, 10f); // Márgenes reducidos
                        PdfWriter writer = PdfWriter.GetInstance(documento, new FileStream(rutaCompleta, FileMode.Create));

                        documento.Open();

                        // Definir fuentes usando Courier
                        BaseFont bf = BaseFont.CreateFont(BaseFont.COURIER, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                        Font fuenteTitulo = new Font(bf, 16, iTextSharp.text.Font.BOLD);  // Fuente para títulos en negrita
                        Font fuenteNormal = new Font(bf, 12, iTextSharp.text.Font.NORMAL);  // Fuente normal
                        Font fuenteNegrita = new Font(bf, 12, iTextSharp.text.Font.BOLD);  // Fuente en negrita para el contenido

                        // Agregar el encabezado
                        Paragraph titulo = new Paragraph("COMITÉ BARRIAL PILACOTO", fuenteTitulo);
                        titulo.Alignment = Element.ALIGN_CENTER;
                        documento.Add(titulo);

                        Paragraph subtitulo = new Paragraph("COMISIÓN CONSTRUCCIÓN IGLESIA", fuenteNegrita);
                        subtitulo.Alignment = Element.ALIGN_CENTER;
                        documento.Add(subtitulo);

                        // Función para agregar párrafos centrados
                        void AgregarParrafoCentrado(string texto)
                        {
                            Paragraph p = new Paragraph(texto, fuenteNormal);
                            p.Alignment = Element.ALIGN_CENTER;
                            documento.Add(p);
                        }

                        AgregarParrafoCentrado("R.U.C: 00000000000000");
                        AgregarParrafoCentrado("PROVINCIA: COTOPAXI CANTON: LATACUNGA");
                        AgregarParrafoCentrado("CIUDAD: PILACOTO COMUNA: PILACOTO");
                        AgregarParrafoCentrado("DIRECCIÓN: PILACOTO");
                        AgregarParrafoCentrado("TELEFONO.: 03-0000000");
                        documento.Add(new Paragraph("\n"));

                        // Agregar los detalles del pago
                        Paragraph tituloPago = new Paragraph("Comprobante de Pago", fuenteNegrita);
                        tituloPago.Alignment = Element.ALIGN_CENTER;
                        documento.Add(tituloPago);

                        documento.Add(new Paragraph("--------------------------------------------------------------------------", fuenteNormal));

                        // Función para agregar detalles con alineación y formato
                        void AgregarDetalle(string etiqueta, string valor)
                        {
                            Paragraph p = new Paragraph();
                            p.Add(new Chunk(etiqueta + ": ", fuenteNegrita));
                            p.Add(new Chunk(valor, fuenteNormal));
                            documento.Add(p);
                        }

                        AgregarDetalle("Código Usuario", codigoUsuario);
                        AgregarDetalle("Cédula", cedula);
                        AgregarDetalle("Nombres", nombres);
                        AgregarDetalle("Fecha de Pago", fechaPago.ToShortDateString());
                        AgregarDetalle("Tipo de Pago", tipoPago);
                        AgregarDetalle("Monto Pagado", montoPago.ToString("C"));

                        documento.Add(new Paragraph("--------------------------------------------------------------------------", fuenteNormal));

                        Paragraph gracias = new Paragraph("Gracias por su pago.", fuenteNegrita);
                        gracias.Alignment = Element.ALIGN_CENTER;
                        documento.Add(gracias);
                        float altoReal = writer.GetVerticalPosition(false) - documento.BottomMargin;
                        documento.SetPageSize(new Rectangle(288f, altoReal));

                        documento.Close();


                        // Confirmar al usuario que el comprobante fue generado
                        MessageBox.Show($"Comprobante PDF generado exitosamente.\nRuta: {rutaCompleta}", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Abrir el PDF generado
                        System.Diagnostics.Process.Start(rutaCompleta);
                    }
                    catch (Exception ex)
                    {
                        // Manejar errores durante la generación del PDF
                        MessageBox.Show($"Error al generar el comprobante PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Seleccione todo los datos requerido", "Dato Requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

            }
            else
            {
                MessageBox.Show("Realice el pago antes de imprimir comprobante", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnReporte_Click(object sender, EventArgs e)
        {
            Form reporte = new Reportes();
            reporte.Show();
        }

       

        private bool ValidarEntrada()
        {
            if (string.IsNullOrEmpty(txtBuscar.Text.Trim()))
            {
                MessageBox.Show("Ingrese criterio a buscar.", "Dato requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;

            }
            return true;
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            Form Agregar = new NuevoUsuario();
            Agregar.Show();
        }

        private void btnDonador_Click(object sender, EventArgs e)
        {
            Form donador = new Donadores();
            donador.Show();
        }


        private void LoadAutoCompleteData()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT UsuarioSistema FROM Usuarios";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                autoCompleteCollection.Add(reader["UsuarioSistema"].ToString());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar datos para autocompletado: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtBuscar.Text) && !long.TryParse(txtBuscar.Text, out _))
            {
                SearchUser(txtBuscar.Text);
            }
        }



        private void SearchUser(string searchTerm)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT TOP 1 CodigoUsuario, Cedula, UsuarioSistema FROM Usuarios WHERE UsuarioSistema LIKE @searchTerm + '%'";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@searchTerm", searchTerm);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtCodigo.Text = reader["CodigoUsuario"].ToString();
                                txtCedula.Text = reader["Cedula"].ToString();
                                txtNombres.Text = reader["UsuarioSistema"].ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al buscar usuario: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


    }
}
