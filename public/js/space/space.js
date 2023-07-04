import API from "../API/API.js";

const backSidebar = document.querySelector(".panel-insert-car");
const Form = document.querySelector(".panel-insert-car form");
const closeWindowCreateUser = document.querySelector(".panel-insert-car .head-panel div > button");
const insertNewUserInSpace = document.querySelector(".panel-insert-car form .more-field .field button");
const Paneltitle = document.querySelector(".panel-insert-car h1");
const cardSpace = document.querySelectorAll(".spaces-estacion .card");
const closeTicket = document.querySelector(".field .close_ticket");
const plac = document.querySelector(".filter-space .more-field .field:nth-child(1) input");
const code = document.querySelector(".filter-space .more-field .field:nth-child(2) input");
const space_status = document.querySelector(".filter-space .more-field .field:nth-child(3) select");
const order = document.querySelector(".filter-space .more-field .field:nth-child(4) select");
const submitForm = document.querySelector(".filter-space button");
const updateSpace = document.querySelector(".update-space");
const spaceResult = document.querySelector("form .more-field > h3");

updateSpace.onclick = function () {
  space();
};

closeWindowCreateUser.onclick = (e) => {
  if (e.target.tagName === "BUTTON" || e.target.tagName === "I") {
    backSidebar.classList.remove("active");
  }
};
space();

async function space() {
  const spaceCOntent = document.querySelector(".spaces-estacion");
  const selectMarca = document.querySelector(".panel-insert-car .more-field .field select#marca");
  const selectModelo = document.querySelector(".panel-insert-car .more-field .field select#modelo");
  const response = await API.spaceService.getSpaces({ plac: plac.value || "", code: code.value || "", space_status: space_status.value || "", order: order.value || "ASC" });
  const marca = await API.spaceService.getMarca();
  spaceResult.textContent = `Resultados: ${response.length}`;

  let stringMarca = "";
  if (marca) {
    marca.map((el) => {
      stringMarca += `<option value="${el.id}">${el.nome}</option>`;
    });
    selectMarca.innerHTML = "<option>Escolher a Marca</option>" + stringMarca;
  }
  console.log(selectMarca);
  selectMarca.onchange = async (e) => {
    const response = await API.spaceService.getModelo(+e.target.value);
    let stringModelo = "";
    // console.log(response);
    if (response) {
      response.map((el) => {
        stringModelo += `<option value="${el.id}">${el.nome}</option>`;
      });
      selectModelo.innerHTML = "<option>Escolher a Modelo</option>" + stringModelo;
    }
  };
  localStorage.setItem("consumers", JSON.stringify(response.filter((r) => r.estado === "i")));
  console.log(marca);
  let stringReceiving = "";
  let status = "";
  let cor = "";
  response.map((el, i) => {
    switch (el.estado) {
      case "i":
        status = "Ocupado";
        cor = "#ff0000";
        break;
      case "a":
        status = "Livre";
        cor = "#1ada74";
        break;
      case "m":
        status = "Indisponível";
        cor = "#ecb900";
        break;

      default:
        status = "???";
        break;
    }
    var tempo = el.tempo_ocupado;
    var temporizador =
      tempo.ano > 0
        ? `${tempo.ano} ano/s e ${tempo.mes} mês/meses`
        : tempo.mes > 0
        ? `${tempo.mes}m e ${tempo.dia}d`
        : tempo.dia > 0
        ? `${tempo.dia}d e ${tempo.hora}h`
        : tempo.hora > 0
        ? `${tempo.hora}h e ${tempo.minuto}min `
        : tempo.minuto > 0
        ? `${tempo.minuto}min e ${tempo.segundo}seg`
        : tempo.segundo > 0
        ? `${tempo.segundo}seg`
        : "";
    stringReceiving += `
      <div class="card" style="background: ${cor};" data-id='${el.id}'>
        <div style='display: flex; justify-content: space-between;'>
          <p class='name' data-id='${el.id}'><strong>${el.nome}</strong></p>
          ${el.preco ? `<p><strong>${el.preco}kz</strong></p>` : ""}
        </div>
        <h1>${status}</h1>
        <div class="card-code">
            <h3>${temporizador ?? "-"}</h3>
        </div>  
      </div>
    `;
  });
  spaceCOntent.innerHTML = stringReceiving;
  setInterval(async () => {
    const response = await API.spaceService.getSpaces({ plac: plac.value || "", code: code.value || "", space_status: space_status.value || "", order: order.value || "ASC" });

    spaceResult.textContent = `Resultados: ${response.length}`;
    localStorage.setItem("consumers", JSON.stringify(response.filter((r) => r.estado === "i")));

    let stringReceiving = "";
    let status = "";
    let cor = "";
    response.map((el, i) => {
      switch (el.estado) {
        case "i":
          status = "Ocupado";
          cor = "#ff0000";
          break;
        case "a":
          status = "Livre";
          cor = "#1ada74";
          break;
        case "m":
          status = "Indisponível";
          cor = "#ecb900";
          break;

        default:
          status = "???";
          break;
      }
      var tempo = el.tempo_ocupado;
      var temporizador =
        tempo.ano > 0
          ? `${tempo.ano} ano/s e ${tempo.mes} mês/meses`
          : tempo.mes > 0
          ? `${tempo.mes}m e ${tempo.dia}d`
          : tempo.dia > 0
          ? `${tempo.dia}d e ${tempo.hora}h`
          : tempo.hora > 0
          ? `${tempo.hora}h e ${tempo.minuto}min `
          : tempo.minuto > 0
          ? `${tempo.minuto}min e ${tempo.segundo}seg`
          : tempo.segundo > 0
          ? `${tempo.segundo}seg`
          : "";

      stringReceiving += `
        <div class="card" style="background: ${cor};" data-id='${el.id}'>
          <div style='display: flex; justify-content: space-between;'>
            <p class='name' data-id='${el.id}'><strong>${el.nome}</strong></p>
            ${el.preco ? `<p><strong>${el.preco}kz</strong></p>` : ""}
          </div>
          <h1>${status}</h1>
          <div class="card-code">
              <h3>${temporizador ?? "-"}</h3>
          </div>
        </div>
      `;
    });
    spaceCOntent.innerHTML = stringReceiving;
    //-------------------------------------------
    spaceCOntent.querySelectorAll(".card").forEach((card) => {
      card.onclick = (e) => {
        const spaceID = card.dataset.id;

        console.log(`${spaceID}`);

        const data = JSON.parse(localStorage.getItem("consumers"));

        const dataConsumer = data.find((kj) => +kj.id === +spaceID);

        if (dataConsumer) {
          closeTicket.setAttribute("data-id", `${dataConsumer.id}.${dataConsumer.bi}`);
          backSidebar.querySelector("form").style.display = "none";
          backSidebar.querySelector(".details").style.display = "block";
          Paneltitle.textContent = "Detalhes";
          showForm();
          const ul = backSidebar.querySelector(".details ul");
          const total = backSidebar.querySelector(".details h1");

          let string = `
            <li>
              <p><strong>Nome:</strong></p>
              <p>${dataConsumer.c_nome}</p>
            </li>
            <li>
              <p><strong>Idade:</strong></p>
              <p>${dataConsumer.idade.ano} anos</p>
            </li>
            <li>
              <p><strong>Bilhete de identidade:</strong></p>
              <p>${dataConsumer.bi}</p>
            </li>
            <li>
              <p><strong>Marca:</strong></p>
              <p>${dataConsumer.marca || "N/A"}</p>
            </li>
            <li>
              <p><strong>Modelo:</strong></p>
              <p>${dataConsumer.modelo || "N/A"}</p>
            </li>
            <li>
              <p><strong>Matrícula:</strong></p>
              <p>${dataConsumer.matricula}</p>
            </li>
            <li>
              <p><strong>Cor:</strong></p>
              <div style='width: 100px; heigth: 30px; background: ${dataConsumer.cor};'></div>
            </li>
          `;
          total.textContent = "Acumulado: " + dataConsumer.preco + "kz";
          ul.innerHTML = string;
        } else {
          backSidebar.querySelector("form").style.display = "block";
          backSidebar.querySelector(".details").style.display = "none";
          showForm();
          Paneltitle.textContent = "Espaço - " + card.querySelector(".name").textContent;
        }

        const space = document.querySelector("input[name=spaceID]");
        space.value = spaceID;
      };
    });
    //-------------------------------------------
  }, 10 * 1000);
  spaceCOntent.querySelectorAll(".card").forEach((card) => {
    card.onclick = (e) => {
      const spaceID = card.dataset.id;
      console.log(`${spaceID}`);

      // console.log(spaceID);
      const data = JSON.parse(localStorage.getItem("consumers"));

      const dataConsumer = data.find((kj) => +kj.id === +spaceID);

      if (dataConsumer) {
        closeTicket.setAttribute("data-id", `${dataConsumer.id}.${dataConsumer.bi}`);
        backSidebar.querySelector("form").style.display = "none";
        backSidebar.querySelector(".details").style.display = "block";
        Paneltitle.textContent = "Detalhes";
        showForm();
        const ul = backSidebar.querySelector(".details ul");
        const total = backSidebar.querySelector(".details h1");

        let string = `
          <li>
            <p><strong>Nome:</strong></p>
            <p>${dataConsumer.c_nome}</p>
          </li>
          <li>
            <p><strong>Idade:</strong></p>
            <p>${dataConsumer.idade.ano} anos</p>
          </li>
          <li>
            <p><strong>Bilhete de identidade:</strong></p>
            <p>${dataConsumer.bi}</p>
          </li>
          <li>
            <p><strong>Marca:</strong></p>
            <p>${dataConsumer.marca || "N/A"}</p>
          </li>
          <li>
            <p><strong>Modelo:</strong></p>
            <p>${dataConsumer.modelo || "N/A"}</p>
          </li>
          <li>
            <p><strong>Matrícula:</strong></p>
            <p>${dataConsumer.matricula}</p>
          </li>
          <li>
            <p><strong>Cor:</strong></p>
            <div style='width: 100px; heigth: 30px; background: ${dataConsumer.cor};'></div>
          </li>
        `;
        total.textContent = "Total: " + dataConsumer.preco + "kz";
        ul.innerHTML = string;
      } else {
        backSidebar.querySelector("form").style.display = "block";
        backSidebar.querySelector(".details").style.display = "none";
        showForm();
        Paneltitle.textContent = "Espaço - " + card.querySelector(".name").textContent;
      }

      const space = document.querySelector("input[name=spaceID]");
      space.value = spaceID;
    };
  });
}

function showForm() {
  backSidebar.classList.add("active");
}
insertNewUserInSpace.onclick = async () => {
  const space = document.querySelector("input[name=spaceID]");
  const formData = new FormData(Form);
  formData.append("spaceID", space.value);
  const response = await API.spaceService.atributeUser(formData);
  console.log(response.data);
  FNtoast(response);

  if (response.status) {
    // let pdf = new jsPDF({
    //   orientation: "landscape",
    //   unit: "in",
    //   format: [2.125, 3.37],
    // });

    // const card = "Cartão :    " + response.user_scanner.nome;
    // const contact = "Contrato : " + response.user_scanner.c_nome;
    // const name = response.user_scanner.marca;
    // pdf.setFont(undefined, "bold");
    // pdf.setFontSize(7);
    // pdf.text(card, 0.1, 1.74);
    // pdf.text(contact, 0.1, 1.87);
    // pdf.text(name.toUpperCase(), 0.1, 2);

    // await pdf.autoPrint({ variant: "non-conform" });
    // await pdf.save(`Cartões-${response.user_scanner.c_nome}-${response.user_scanner.code}.pdf`);

    // return;
    // Abrir uma nova janela e exibir o relatório
    let printWindow = window.open("", "_blank", "status=1,width=550,height=550");
    printWindow.document.open();

    printWindow.document.write(
      "<!DOCTYPE html><html><head><title>Ticket</title><style>.container {transform: scale(2); width: 300px;margin: 50% auto 0;text-align: center;text-transform: uppercase;}.container h1 {font-size: 50px;}.container p:nth-child(2) {font-size: 14px;}</style></head><body><div class='container'><h1>" +
        response.user_scanner.nome +
        "</h1><p>" +
        response.user_scanner.c_nome +
        "</p><p>" +
        response.user_scanner.marca +
        " - " +
        response.user_scanner.modelo +
        "</p><p>" +
        response.user_scanner.data_entrada +
        "</p><p>" +
        response.user_scanner.code +
        "</p>" +
        "</div><script>window.onload = () => {window.print();};</script></body></html>"
    );
    // printWindow.onload = function () {
    printWindow.print();
    // };
    // document.body.innerHTML = originalContents;
  }

  // space();
};

/* var allSpace = [];
const alphabet = "ABCDEFGHIJKLMNOQRSTUVWXYZ".split("");
for (var i = 0; i < alphabet.length; i++){
    var t = 1;
    for(; t <= 10; t++){
        allSpace.push(alphabet[i]+t)
    }
} */

function FNtoast(response) {
  const toast = document.querySelector(".drt .toast");
  const icon = toast.querySelector(".icon i");

  toast.classList.add(response.status ? "success" : "error");
  icon.classList.add(response.status ? "fa-check" : "fa-times");

  toast.querySelector(".text p").textContent = response.message;
  setTimeout(() => {
    icon.classList.remove(response.status ? "fa-check" : "fa-times");
    toast.classList.remove(response.status ? "success" : "error");
    console.log(toast.classList);
  }, 6000);
}

closeTicket.onclick = async () => {
  const id = closeTicket.dataset.id;
  const Ok = await API.spaceService.closeTicket(id);
  FNtoast(Ok);
  Ok.status && backSidebar.classList.remove("active");
};

submitForm.onclick = async () => {
  const data = { plac: plac.value || "", code: code.value || "", space_status: space_status.value || "", order: order.value || "ASC" };
  console.log(data);
  await API.spaceService.getSpaces(data);
  space();
};
