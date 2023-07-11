const body = document.querySelector("body");
const sun = document.querySelector(".sun");
const moon = document.querySelector(".moon");
const activingMode = document.querySelector(".drt .footer .set-dark-theme .activing");

window.onload = () => {
  if (!localStorage.getItem("darkTheme")) {
    localStorage.setItem("darkTheme", false);
    body.classList.add(JSON.parse(localStorage.getItem("darkTheme")) && "dark-mode");
    JSON.parse(localStorage.getItem("darkTheme")) && activingMode.classList.add("dark");
  } else {
    body.classList.add(JSON.parse(localStorage.getItem("darkTheme")) && "dark-mode");
    JSON.parse(localStorage.getItem("darkTheme")) && activingMode.classList.add("dark");
  }
};
sun.onclick = () => {
  localStorage.setItem("darkTheme", false);
  body.classList.remove("dark-mode");
  activingMode.classList.remove("dark");
};
moon.onclick = () => {
  localStorage.setItem("darkTheme", true);
  body.classList.add(JSON.parse(localStorage.getItem("darkTheme")) && "dark-mode");
  activingMode.classList.add("dark");
};

// --------------------------------
const closeSession = document.querySelector(".disconnect button");

// close session or clean cookie where name is token
closeSession.onclick = async (e) => {
  location.href = "/session-close";
};

/* criar uma funcao que pega o indice da palavra token e depois pegar o ; e verificar se este termina com ;,
 se for o caso então continua senão retira o ; como segundo parâmetro */

function checkCookie(cookieName = "") {
  var cookieFound = "";

  const splitCookie = document.cookie.substring(document.cookie.indexOf(cookieName + "=")).split("");

  for (var op = 0; op < splitCookie.length; op++) {
    cookieFound += splitCookie[op];
    if (splitCookie[op] == ";") {
      op = splitCookie.length;
    }
  }

  document.cookie = document.cookie.replace(cookieFound, `${cookieName}=; expires=Thu, 01 Jan 1970 00:00:00 GMT`);
  console.log(cookieFound);
}
