import os
import decouple
import dashscope

dashscope.base_http_api_url = "https://dashscope-intl.aliyuncs.com/api/v1"
api_key: str | None = decouple.config("DASHSCOPE_API_KEY", default=None)  # type: ignore
assert api_key is not None, "DASHSCOPE_API_KEY environment variable is not set."

video_path = os.path.abspath("./data/The Ice Maples!.mp4")
messages = [
    {
        "role": "system",
        "content": """
You are an assistant specialized in analyzing videos of speed skating short track events.
You can identify skaters and provide insights about key moments in the race.

You first analyse the entrie video, then extract all key moments that are relevant to the race or cause changes in the race dynamics.

Your answers are structured in a json format:
{
  "raceId": "seoul-men500-finalA",
  "videoOffset": <number>,  // offset in seconds if the race does not start at the beginning of the race
  "moments": [
    {
      "title": <string>,
      "description": "<string>,",
      "triggerTime": <number>,  // time in seconds from the start of the video
      "duration": <number>  // duration in seconds
    },
    ... // more key moments
  ]
}

You are not allowed to fabricate any information. If certain details are not present in the video, respond with "unknown" for those fields.
Your answers must strictly adhere to the specified json format. Do not include any additional commentary or explanations outside of the json structure.

        """.strip(),
    },
    {
        "role": "user",
        "content": [
            # {
            #     "image": "https://upload.wikimedia.org/wikipedia/commons/thumb/2/2b/Photographer_Photographing_Nevada_Mountains.jpg/960px-Photographer_Photographing_Nevada_Mountains.jpg"
            # },
            # {
            #     "video": "https://help-static-aliyun-doc.aliyuncs.com/file-manage-files/zh-CN/20241115/cqqkru/1.mp4",
            #     "fps": 2,
            # },
            {"video": f"file://{video_path}", "fps": 10},
            {"text": "analyse this video"},
        ],
    },
]

response = dashscope.MultiModalConversation.call(
    api_key=api_key,
    model="qwen3-vl-plus",
    messages=messages,
    stream=True,
    incremental_output=True,
    thinking_budget=4000,
    enable_thinking=True,
)


is_answering = False
reasoning_content = ""
answer_content = ""
print("Streaming output content:")
for chunk in response:
    message = chunk.output.choices[0].message
    reasoning_content_chunk = message.get("reasoning_content", None)
    if message.content == [] and reasoning_content_chunk == "":
        continue

    if reasoning_content_chunk != None:
        print(message.reasoning_content, end="")
        reasoning_content += message.reasoning_content
        continue

    if message.content != []:
        if not is_answering:
            print("\n" + "=" * 20 + "Complete Response" + "=" * 20)
            is_answering = True
        print(message.content[0]["text"], end="")
        answer_content += message.content[0]["text"]

with open("output.json", "w", encoding="utf-8") as f:
    f.write(answer_content)
