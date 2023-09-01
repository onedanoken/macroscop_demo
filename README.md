# Macroscop. Тестовое задание
Формулировка задания:
Разработать Windows Desktop приложение на .NET (версия 6 или выше), позволяющее просматривать видео реального времени с одной из камер, подключенных к серверу видеонаблюдения Macroscop. Сервер находится по адресу http://demo.macroscop.com:8080/ и имеет открытый HTTP API,  позволяющий сторонним системам получать список доступных камер и видео с них.
Сервер может выполнять следующие запросы:
1.	Запрос конфигурации
Пример: http://demo.macroscop.com:8080/configex?login=root
В ответ отправляется XML-документ, содержащий, в частности, все доступные камеры (элементы ChannelInfo), их имена (поля Name) и идентификаторы (поля Id).
2.	Запрос видео
Пример: http://demo.macroscop.com:8080/mobile?login=root&channelid=2016897c-8be5-4a80-b1a3-7f79a9ec729c&resolutionX=640&resolutionY=480&fps=25
В ответ на запрос сервер будет возвращать видео с камеры в формате MJPEG, имеющей идентификатор, заданный в параметре channelid.  Просмотреть видео из примера возможно с помощью программы VLC Player (http://www.videolan.org/).

# Установка и запуск приложения
Приложение создано на .NET 7.0 WPF C#. Рекомендуется запускать его через Visual Studio 2022.
Для начала необходимо склонировать данный репозиторий:
```
git clone https://github.com/onedanoken/macroscop_demo.git
```
После этого в папке macroscop_demo необходимо запустить VideoCaptureApplication.sln.
После этого проект должен собраться и быть запущен. В случае возникновения проблем (ошибок компиляции и т.д.), просьба сообщить.
